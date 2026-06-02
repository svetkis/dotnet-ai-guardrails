namespace DemoProject.Domain;

// TRAP: Агент добавляет тяжёлые аллокации или async в метод, который вызывается 1000 раз в секунду.
// GUARDRAIL: [HotPath] + Roslyn-анализатор ловит new/async/boxing до запуска тестов.
[AttributeUsage(AttributeTargets.Method)]
public sealed class HotPathAttribute : Attribute { }
