// GUARDRAIL: NBomber показывает, что $Max$ latency не деградировал.
// Этот файл — рабочая адаптация шаблона из tests/patterns/LoadTest.cs
// Для демонстрации используем in-memory нагрузку без внешних HTTP-зависимостей.

using NBomber.Contracts;
using NBomber.CSharp;
using TUnit;

namespace DemoProject.Tests;

public class LoadTests
{
    [Test]
    public async Task InMemoryReadWrite_ShouldNotDegradeLatency()
    {
        var readScenario = Scenario.Create("read_bookings", async context =>
        {
            var svc = new DemoProject.Application.BookingService();
            var result = await svc.GetPendingAsync();
            return result.Count >= 0 ? Response.Ok() : Response.Fail();
        })
        .WithoutWarmUp()
        .WithLoadSimulations(Simulation.Inject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(5)));

        var writeScenario = Scenario.Create("confirm_booking", async context =>
        {
            var svc = new DemoProject.Application.BookingService();
            await svc.ConfirmAsync(Guid.NewGuid());
            return Response.Ok();
        })
        .WithoutWarmUp()
        .WithLoadSimulations(Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(5)));

        var stats = NBomberRunner
            .RegisterScenarios(readScenario, writeScenario)
            .Run();

        var writeStats = stats.ScenarioStats.First(s => s.ScenarioName == "confirm_booking");

        await Assert.That(writeStats.Ok.Latency.MaxMs).IsLessThanOrEqualTo(1000)
            .Because("Max write latency must not degrade under load.");

        await Assert.That(writeStats.Fail.Request.Percent).IsEqualTo(0)
            .Because("No write requests should fail.");
    }
}
