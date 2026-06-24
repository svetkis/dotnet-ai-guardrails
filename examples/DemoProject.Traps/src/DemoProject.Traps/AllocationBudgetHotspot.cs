namespace DemoProject.Traps;

public sealed class AllocationBudgetHotspot
{
    // TRAP: Агент добавил new в [HotPath] метод.
    // GUARDRAIL: AllocationBudgetTests ловит регресс аллокаций.
    // NOTE: Этот метод специально аллоцирует, чтобы показать падение guardrail.
    [HotPath]
    public int Process(int value)
    {
        var list = new List<int> { value };
        return list.Count;
    }
}
