// TRAP: Агент при рефакторинге тихо сносит [HotPath] атрибуты или критичные методы.
// GUARDRAIL: Рефлексией считаем методы с [HotPath]. Если count уменьшился — агент что-то сломал.

using System.Reflection;
using TUnit;

namespace Tests.Patterns;

public class RatchetTests
{
    // TRAP: Агент удалил [HotPath] при "cleanup", думая что атрибут не нужен.
    // GUARDRAIL: Этот тест падает, если количество hot paths уменьшилось.
    [Test]
    public void HotPathCount_ShouldNotDecrease()
    {
        // Arrange
        var assembly = typeof(YourApplicationAssembly).Assembly;
        var currentCount = CountHotPaths(assembly);

        // Базовое значение — зафиксировано вручную при аудите
        const int baselineCount = 12;

        // Assert
        Assert.That(currentCount).IsGreaterThanOrEqualTo(baselineCount);
    }

    // TRAP: Агент добавил метод с [HotPath], но не покрыл его тестами.
    // GUARDRAIL: Каждый [HotPath] должен иметь хотя бы один unit-test.
    [Test]
    public void AllHotPaths_ShouldHaveTests()
    {
        var assembly = typeof(YourApplicationAssembly).Assembly;
        var hotPaths = GetHotPathMethods(assembly);
        var testMethods = GetTestMethods(typeof(RatchetTests).Assembly);

        foreach (var hotPath in hotPaths)
        {
            var hasTest = testMethods.Any(t =>
                t.Name.Contains(hotPath.Name, StringComparison.OrdinalIgnoreCase));

            Assert.That(hasTest).IsTrue();
        }
    }

    private static int CountHotPaths(Assembly assembly)
    {
        return GetHotPathMethods(assembly).Count();
    }

    private static IEnumerable<MethodInfo> GetHotPathMethods(Assembly assembly)
    {
        return assembly.GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            .Where(m => m.GetCustomAttribute<HotPathAttribute>() != null);
    }

    private static IEnumerable<MethodInfo> GetTestMethods(Assembly assembly)
    {
        return assembly.GetTypes()
            .SelectMany(t => t.GetMethods())
            .Where(m => m.GetCustomAttribute<TestAttribute>() != null);
    }
}

// Атрибут для маркировки критичных путей
[AttributeUsage(AttributeTargets.Method)]
public class HotPathAttribute : Attribute { }
