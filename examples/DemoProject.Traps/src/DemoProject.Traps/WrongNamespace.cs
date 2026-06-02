namespace DemoProject.Traps.Application;

// TRAP: Агент создал файл в папке Domain, но namespace указывает на Application.
// GUARDRAIL: HaveSourceFilePathMatchingNamespace ловит рассогласование.
public class WrongNamespace
{
}
