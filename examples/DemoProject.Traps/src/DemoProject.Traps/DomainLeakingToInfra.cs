using System.Net.Http;

namespace DemoProject.Traps.Domain;

// TRAP: Агент добавил using System.Net.Http в Domain для "одного вызова".
// GUARDRAIL: HaveDependencyOnAny ловит IL-зависимость от запрещённого namespace.
public class DomainLeakingToInfra
{
    public void DoSomething()
    {
        _ = new HttpClient();
    }
}
