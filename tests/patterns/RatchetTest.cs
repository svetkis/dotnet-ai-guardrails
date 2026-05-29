// TRAP: Агент при рефакторинге тихо удаляет типы, сервисы или ломает test runner.
// GUARDRAIL: Рефлексией считаем публичные типы и тесты. Если count уменьшился — агент что-то сломал.

using System.Reflection;
using TUnit;

namespace Tests.Patterns;

public class RatchetTests
{
    // TRAP: Агент удалил сервисы или DTO при "cleanup", думая что они не нужны.
    // GUARDRAIL: Этот тест падает, если количество публичных типов в слое уменьшилось.
    [Test]
    public void PublicTypeCount_ShouldNotDecrease()
    {
        // Arrange
        var assembly = typeof(YourApplicationAssembly).Assembly;
        var currentCount = CountPublicTypes(assembly);

        // Базовое значение — зафиксировано вручную при аудите
        const int baselineCount = 12;

        // Assert
        Assert.That(currentCount).IsGreaterThanOrEqualTo(baselineCount);
    }

    // TRAP: Агент сломал test runner или удалил тестовый проект — "0 tests ran, exit 0".
    // GUARDRAIL: Архитектурный тест проверяет, что количество тестов не уменьшилось.
    [Test]
    public void TestCount_ShouldNotDecrease()
    {
        var testAssembly = typeof(RatchetTests).Assembly;
        var currentCount = GetTestMethods(testAssembly).Count();

        // Базовое значение — зафиксировано при аудите. Обновлять вручную после роста покрытия.
        const int baselineCount = 10;

        Assert.That(currentCount).IsGreaterThanOrEqualTo(baselineCount)
            .Because("Test count must not silently decrease. If runner breaks, this catches it.");
    }

    private static int CountPublicTypes(Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(t => t.IsPublic && !t.IsNested)
            .Count();
    }

    private static IEnumerable<MethodInfo> GetTestMethods(Assembly assembly)
    {
        return assembly.GetTypes()
            .SelectMany(t => t.GetMethods())
            .Where(m => m.GetCustomAttribute<TestAttribute>() != null);
    }
}
