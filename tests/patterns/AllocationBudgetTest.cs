// TRAP: Агент добавляет new/async/boxing в метод с [HotPath], и latency деградирует на проде.
// GUARDRAIL: Каждый [HotPath] метод имеет allocation-бюджет; регресс ловится в тестах.
//
// Адаптация под фреймворк:
// - TUnit:  [Test] + Assert.That(...)
// - xUnit:  [Fact] + Assert.True(...)
// - NUnit:  [Test] + Assert.That(...)
// - MSTest: [TestMethod] + Assert.IsTrue(...)
//
// NOTE: Для стабильности запускайте в изолированной среде (одинаковый OS, .NET runtime, GC mode).
//       Используйте warmup + несколько итераций, чтобы избежать flaky тестов.

using System.Reflection;
using TUnit;

namespace Tests.Patterns;

// Маркер hot path. Можно заменить на свой атрибут из проекта.
[AttributeUsage(AttributeTargets.Method)]
public class HotPathAttribute : Attribute { }

public class AllocationBudgetTests
{
    // TRAP: Агент добавил лишние аллокации в критичный метод.
    // GUARDRAIL: Аллокации [HotPath] метода не превышают baseline + 10%.
    [Test]
    public void HotPath_GetAvailableSlots_AllocationBudget()
    {
        var budget = MeasureAllocationBudget(
            action: () => YourHotPathService.GetAvailableSlots(DateTime.UtcNow),
            warmupIterations: 3,
            measureIterations: 100);

        // Baseline зафиксирован при первом аудите. Обновлять вручную после осознанной оптимизации.
        const long baselineBytes = 1024;
        var threshold = (long)(baselineBytes * 1.10);

        Assert.That(budget.BytesAllocated)
            .IsLessThanOrEqualTo(threshold)
            .Because($"Hot path allocations must not exceed baseline + 10%. " +
                     $"Baseline={baselineBytes}, Current={budget.BytesAllocated}, Threshold={threshold}");
    }

    // TRAP: Агент добавил [HotPath] метод, но забыл написать для него allocation-тест.
    // GUARDRAIL: Каждый public метод с [HotPath] имеет парный тест {MethodName}_AllocationBudget.
    [Test]
    public void AllHotPathMethods_HaveAllocationBudgetTests()
    {
        var hotPathMethods = GetHotPathMethods(typeof(YourHotPathService).Assembly);
        var testMethods = GetTestMethods(typeof(AllocationBudgetTests).Assembly)
            .Select(m => m.Name)
            .ToHashSet();

        var missing = hotPathMethods
            .Where(m => !testMethods.Contains($"{m.Name}_AllocationBudget"))
            .Select(m => $"{m.DeclaringType?.FullName}.{m.Name}")
            .ToList();

        Assert.That(missing).IsEmpty()
            .Because("Every [HotPath] method must have a matching {MethodName}_AllocationBudget test.");
    }

    // --- Helpers ---

    private static AllocationBudget MeasureAllocationBudget(Action action, int warmupIterations, int measureIterations)
    {
        // Warmup
        for (var i = 0; i < warmupIterations; i++)
            action();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < measureIterations; i++)
            action();
        var after = GC.GetAllocatedBytesForCurrentThread();

        return new AllocationBudget(after - before);
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

    private readonly record struct AllocationBudget(long BytesAllocated);
}
