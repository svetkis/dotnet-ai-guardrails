using DemoProject.Domain;

namespace DemoProject.Infrastructure;

public sealed class InfrastructureBookingService : IBookingService
{
    // TRAP: Агент может забыть про CancellationToken или использовать FindAsync в read-path.
    // GUARDRAIL: Code review agent + ArchitectureRules.FindAsync_ShouldNotBeUsedInReadPath

    public Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return Task.FromResult<Booking?>(null);
    }

    public Task<IReadOnlyList<Booking>> GetPendingAsync(CancellationToken ct = default)
    {
        return Task.FromResult<IReadOnlyList<Booking>>(Array.Empty<Booking>());
    }

    public Task ConfirmAsync(Guid id, CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }
}
