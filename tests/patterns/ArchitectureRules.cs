// TRAP: Агент использует FindAsync в read-path или нарушает слоистую архитектуру.
// GUARDRAIL: NetArchTest проверяет правила архитектуры на уровне сборки.

using NetArchTest.Rules;
using TUnit;

namespace Tests.Patterns;

public class ArchitectureRules
{
    // TRAP: Агент добавил FindAsync в query-handler, "потому что так короче".
    // GUARDRAIL: FindAsync допустим только в Command-handlers (write-path).
    [Test]
    public void FindAsync_ShouldNotBeUsedInReadPath()
    {
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace(".*Queries.*")
            .Should().Not().HaveMethodNameMatching("FindAsync")
            .GetResult();

        Assert.That(result.IsSuccessful).IsTrue();
    }

    // TRAP: Агент зареференсил Infrastructure из Api напрямую.
    // GUARDRAIL: Api → Application → Domain. Infrastructure только через DI.
    [Test]
    public void Api_ShouldNotReferenceInfrastructureDirectly()
    {
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace(".*Api.*")
            .Should().Not().DependOnAny(Types.That().ResideInNamespace(".*Infrastructure.*"))
            .GetResult();

        Assert.That(result.IsSuccessful).IsTrue();
    }

    // TRAP: Агент добавил DbContext в Application layer.
    // GUARDRAIL: Application знает только про Ports (интерфейсы).
    [Test]
    public void Application_ShouldNotReferenceEfCore()
    {
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace(".*Application.*")
            .Should().Not().DependOnAny(Types.That().HaveNameStartingWith("Microsoft.EntityFrameworkCore"))
            .GetResult();

        Assert.That(result.IsSuccessful).IsTrue();
    }

    // TRAP: Агент создал сервис без интерфейса в Application.
    // GUARDRAIL: Все сервисы должны иметь интерфейс (Port).
    [Test]
    public void Services_ShouldHaveInterfaces()
    {
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace(".*Infrastructure.*")
            .And().HaveNameEndingWith("Service")
            .Should().ImplementInterface(typeof(IService))
            .GetResult();

        Assert.That(result.IsSuccessful).IsTrue();
    }
}
