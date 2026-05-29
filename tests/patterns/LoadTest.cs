// TRAP: Агент оптимизирует read через AsNoTracking, но ломает write.
// GUARDRAIL: NBomber показывает, что $Max$ write-операций деградировал.

using NBomber.Contracts;
using NBomber.CSharp;
using TUnit;

namespace Tests.Patterns;

public class LoadTests
{
    // TRAP: Агент добавил AsNoTracking в GetPendingItems.
    // InMemory тест проходит, но на проде Status не сохраняется.
    // GUARDRAIL: Гоняем read + write под нагрузкой. $Max$ write не должен вырасти.
    [Test]
    public void ReadWriteMix_ShouldNotDegradeWriteLatency()
    {
        var httpClient = new HttpClient();

        var readScenario = Scenario.Create("read_items", async context =>
        {
            var response = await httpClient.GetAsync("/api/items/pending");
            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        .WithoutWarmUp()
        .WithLoadSimulations(Simulation.Inject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30)));

        var writeScenario = Scenario.Create("write_status", async context =>
        {
            var response = await httpClient.PostAsJsonAsync("/api/items/1/confirm", new { });
            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        .WithoutWarmUp()
        .WithLoadSimulations(Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30)));

        var stats = NBomberRunner
            .RegisterScenarios(readScenario, writeScenario)
            .Run();

        // GUARDRAIL: $Max$ latency write-операций не должен превышать 500ms
        var writeStats = stats.ScenarioStats.First(s => s.ScenarioName == "write_status");
        Assert.That(writeStats.Ok.Latency.MaxMs).IsLessThanOrEqualTo(500);

        // GUARDRAIL: Не должно быть failed запросов (иначе state machine сломан)
        Assert.That(writeStats.Fail.Count).IsEqualTo(0);
    }
}
