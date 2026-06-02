namespace DemoProject.Traps.Domain;

// TRAP: Агент добавил mutable state в Domain через public field.
// GUARDRAIL: BeImmutableExternally ловит public fields.
public class MutableState
{
    public int Counter;
}
