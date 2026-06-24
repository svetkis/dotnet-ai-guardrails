// GUARDRAIL: Рефлексией считаем публичные типы и тесты. Если count уменьшился — агент что-то сломал.
// Этот файл — рабочая адаптация шаблона из tests/patterns/RatchetTest.cs

using System.Reflection;
using TUnit;

namespace DemoProject.Tests;

public class RatchetTests
{
    private static readonly Assembly ApplicationAssembly = typeof(Application.BookingService).Assembly;
    private static readonly Assembly TestAssembly = typeof(RatchetTests).Assembly;

    [Test]
    public async Task PublicTypeCount_ShouldNotDecrease()
    {
        var currentCount = CountPublicTypes(ApplicationAssembly);
        const int baselineCount = 1; // BookingService

        await Assert.That(currentCount).IsGreaterThanOrEqualTo(baselineCount)
            .Because("Agent must not silently remove public types during refactoring.");
    }

    [Test]
    public async Task TestCount_ShouldNotDecrease()
    {
        var currentCount = GetTestMethods(TestAssembly).Count();
        const int baselineCount = 5; // Update after adding tests

        await Assert.That(currentCount).IsGreaterThanOrEqualTo(baselineCount)
            .Because("Test count must not silently decrease. If runner breaks, this catches it.");
    }

    private static int CountPublicTypes(Assembly assembly)
    {
        return assembly.GetTypes()
            .Count(t => t.IsPublic && !t.IsNested);
    }

    private static IEnumerable<MethodInfo> GetTestMethods(Assembly assembly)
    {
        return assembly.GetTypes()
            .SelectMany(t => t.GetMethods())
            .Where(m => m.GetCustomAttribute<TestAttribute>() != null);
    }
}
