// TRAP: Агент возвращает Domain Entity из Application-сервиса вместо DTO.
// GUARDRAIL: NetArchTest custom rule (Mono.Cecil) ловит Entity leak в сигнатурах методов.
// NOTE: Entity leak — скрытый цикл: Application тянет ORM-логику, lazy loading,
//       навигационные свойства, и тестировать слой изолированно невозможно.

using Mono.Cecil;
using NetArchTest.Rules;
using TUnit;

namespace Tests.Patterns;

public class EntityLeakTest
{
    // TRAP: Агент написал Task<Booking> вместо Task<BookingDto>.
    // GUARDRAIL: Application-интерфейсы не возвращают Domain Entity.
    [Test]
    public void ApplicationInterfaces_ShouldNotReturnDomainEntities()
    {
        // Адаптация: замените на свои assembly и convention для Entity.
        // var appAssembly = typeof(YourApplicationService).Assembly;
        // var entityRule = new EntityLeakRule(entityNamespace: "YourProject.Domain", excludedSuffixes: new[] { "Dto", "ViewModel" });
        //
        // var result = Types.InAssembly(appAssembly)
        //     .That().ResideInNamespace(".*Application.*")
        //     .And().AreInterfaces()
        //     .Should().MeetCustomRule(entityRule)
        //     .GetResult();
        //
        // Assert.That(result.IsSuccessful).IsTrue();

        Assert.That(true).IsTrue()
            .Because("Template: adapt this test to your assembly and entity naming convention. See commented code.");
    }

    // TRAP: Агент добавил Entity leak, но тест проходит потому что baseline не обновлён.
    // GUARDRAIL: Ratchet — считаем текущее количество нарушений и проверяем, что оно не растёт.
    [Test]
    public void ApplicationInterfaces_EntityLeakCount_ShouldNotGrow()
    {
        // Адаптация:
        // var appAssembly = typeof(YourApplicationService).Assembly;
        // var entityRule = new EntityLeakRule(...);
        // var violations = CountViolations(appAssembly, entityRule);
        //
        // const int baseline = 0; // Зафиксируй текущее значение
        // Assert.That(violations).IsLessThanOrEqualTo(baseline)
        //     .Because($"Entity leaks must not grow. Current: {violations}, baseline: {baseline}");

        Assert.That(true).IsTrue()
            .Because("Template: adapt this test to count violations. See commented code.");
    }

    // --- Custom Rule: detects methods returning types from forbidden namespace ---
    public class EntityLeakRule : ICustomRule
    {
        private readonly string _entityNamespace;
        private readonly string[] _excludedSuffixes;

        public EntityLeakRule(string entityNamespace, string[] excludedSuffixes)
        {
            _entityNamespace = entityNamespace;
            _excludedSuffixes = excludedSuffixes;
        }

        public bool MeetsRule(TypeDefinition type)
        {
            if (!type.IsInterface)
                return true;

            foreach (var method in type.Methods)
            {
                var returnTypeName = GetTypeName(method.ReturnType);
                if (IsEntity(returnTypeName))
                    return false;
            }

            return true;
        }

        private bool IsEntity(string typeName)
        {
            if (!typeName.Contains(_entityNamespace))
                return false;

            return !_excludedSuffixes.Any(suffix => typeName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
        }

        private static string GetTypeName(TypeReference typeRef)
        {
            // Unwrap Task<T>, IEnumerable<T>, etc.
            if (typeRef is GenericInstanceType generic)
            {
                var elementType = generic.ElementType?.FullName ?? "";
                if (elementType.StartsWith("System.Threading.Tasks.Task`1", StringComparison.OrdinalIgnoreCase)
                    || elementType.StartsWith("System.Collections.Generic.IEnumerable`1", StringComparison.OrdinalIgnoreCase)
                    || elementType.StartsWith("System.Collections.Generic.IReadOnlyList`1", StringComparison.OrdinalIgnoreCase)
                    || elementType.StartsWith("System.Collections.Generic.IReadOnlyCollection`1", StringComparison.OrdinalIgnoreCase)
                    || elementType.StartsWith("System.Collections.Generic.List`1", StringComparison.OrdinalIgnoreCase))
                {
                    var arg = generic.GenericArguments.FirstOrDefault();
                    return arg?.FullName ?? typeRef.FullName;
                }
            }

            return typeRef.FullName;
        }
    }
}
