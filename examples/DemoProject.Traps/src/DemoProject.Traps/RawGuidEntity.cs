namespace DemoProject.Traps.Domain;

// TRAP: Агент использовал Guid вместо strongly typed ID.
// GUARDRAIL: Regex + архитектурные тесты ловят сырые Guid в именах свойств.
public class RawGuidEntity
{
    public Guid Id { get; init; }
    public Guid ClientId { get; init; }
}
