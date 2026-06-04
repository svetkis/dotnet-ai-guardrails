// TRAP: Агент в single-project нарушает naming, использует banned APIs
// или забывает CancellationToken в public async методах.
// GUARDRAIL: NetArchTest + regex-сканирование ловят нарушения конвенций
// даже когда нет слоёв Clean Architecture.
//
// Адаптация под фреймворк:
// - TUnit:  [Test] + Assert.That(result.IsSuccessful).IsTrue()
// - xUnit:  [Fact] + Assert.True(result.IsSuccessful)
// - NUnit:  [Test] + Assert.That(result.IsSuccessful, Is.True)
// - MSTest: [TestMethod] + Assert.IsTrue(result.IsSuccessful)

using System.Reflection;
using System.Text.RegularExpressions;
using DemoProject.MinimalApi.Features.Orders;
using NetArchTest.Rules;
using TUnit;

namespace DemoProject.MinimalApi.Tests;

public class ArchitectureRules
{
    private static readonly Assembly AppAssembly = typeof(OrderService).Assembly;

    // TRAP: Агент создал сервис с неправильным именем (например, OrderManager).
    // GUARDRAIL: Все сервисы должны заканчиваться на "Service".
    [Test]
    public async Task Services_ShouldHaveNameEndingWithService()
    {
        // Альтернативная проверка: найти типы, которые НЕ заканчиваются на Service, но содержат бизнес-логику
        var violations = Types.InAssembly(AppAssembly)
            .That().DoNotHaveNameEndingWith("Service")
            .And().DoNotHaveNameEndingWith("Endpoints")
            .And().DoNotHaveNameEndingWith("Request")
            .And().DoNotHaveNameEndingWith("Response")
            .And().DoNotHaveNameEndingWith("Program")
            .And().DoNotResideInNamespace(".*Domain.*")
            .And().AreClasses()
            .And().AreNotAbstract()
            .GetTypes()
            .ToList();

        // NetArchTest.GetTypes() возвращает IType, у которого нет IsNested/IsPublic.
        // Фильтруем через рефлексию после получения имён.
        var typeNames = violations.Select(v => v.FullName).ToList();
        var reflectedTypes = typeNames
            .Select(name => AppAssembly.GetType(name))
            .Where(t => t is not null && t.IsPublic && !t.IsNested && !t.Namespace!.StartsWith("Microsoft") && !t.Namespace!.Contains(".Domain"))
            .ToList();

        await Assert.That(reflectedTypes).IsEmpty()
            .Because($"Public classes outside Domain must end with Service, Endpoints, Request, or Response. Violations: {string.Join(", ", reflectedTypes.Select(v => v!.Name))}");
    }

    // TRAP: Агент использовал DateTime.Now вместо UtcNow.
    // GUARDRAIL: Regex-сканирование ловит banned API.
    [Test]
    public async Task SourceCode_ShouldNotUse_DateTimeNow()
    {
        var violations = ScanSourceFiles(
            pattern: @"DateTime\.Now\b",
            fileGlob: "*.cs",
            whitelist: new[] { "OrderService.cs: comment only" });

        await Assert.That(violations).IsEmpty()
            .Because("Use DateTime.UtcNow or IClock abstraction. DateTime.Now causes timezone bugs.");
    }

    // TRAP: Агент добавил public async метод без CancellationToken.
    // GUARDRAIL: Рефлексия проверяет все public async методы.
    [Test]
    public async Task PublicAsyncMethods_ShouldAcceptCancellationToken()
    {
        var violations = AppAssembly.GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            .Where(m => m.IsPublic && IsAsyncMethod(m))
            .Where(m => !m.GetParameters().Any(p => p.ParameterType == typeof(CancellationToken)))
            .Select(m => $"{m.DeclaringType?.Name}.{m.Name}")
            .ToList();

        await Assert.That(violations).IsEmpty()
            .Because($"Every public async method must accept CancellationToken ct = default. Violations: {string.Join(", ", violations)}");
    }

    // TRAP: Агент добавил using System.Net.Http в Domain для "одного вызова".
    // GUARDRAIL: Даже в single-project Domain namespace не должен зависеть от инфраструктуры.
    [Test]
    public async Task DomainNamespace_ShouldNotReferenceInfrastructure()
    {
        var result = Types.InAssembly(AppAssembly)
            .That().ResideInNamespace(".*Domain.*")
            .Should()
            .NotHaveDependencyOnAny("System.Net.Http", "System.Data.SqlClient")
            .GetResult();

        await Assert.That(result.IsSuccessful).IsTrue()
            .Because("Domain must not depend on infrastructure namespaces like System.Net.Http or System.Data.SqlClient");
    }

    private static bool IsAsyncMethod(MethodInfo method)
    {
        return method.ReturnType == typeof(Task)
            || (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
            || method.ReturnType == typeof(ValueTask)
            || (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(ValueTask<>));
    }

    private static IEnumerable<string> ScanSourceFiles(string pattern, string fileGlob, string[] whitelist)
    {
        var srcPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src");
        srcPath = Path.GetFullPath(srcPath);

        if (!Directory.Exists(srcPath))
            return Array.Empty<string>();

        var files = Directory.GetFiles(srcPath, fileGlob, SearchOption.AllDirectories);
        var violations = new List<string>();
        var regex = new Regex(pattern);

        foreach (var file in files)
        {
            if (file.Contains("obj") || file.Contains("bin"))
                continue;

            var lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
            {
                if (regex.IsMatch(lines[i]))
                {
                    var relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), file);
                    var location = $"{relativePath}:{i + 1}";

                    if (whitelist.Any(w => location.Contains(w.Split(':')[0])))
                        continue;

                    violations.Add(location);
                }
            }
        }

        return violations;
    }
}
