namespace DemoProject.Traps;

// TRAP: Агент добавляет тяжёлые аллокации в метод, который вызывается часто.
// GUARDRAIL: [HotPath] + AllocationBudgetTests ловит регресс аллокаций.
[AttributeUsage(AttributeTargets.Method)]
public sealed class HotPathAttribute : Attribute { }
