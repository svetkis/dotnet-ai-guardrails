using DemoProject.Domain;

namespace DemoProject.Application;

public sealed class BookingService : IBookingService
{
    // TRAP: Агент может добавить using DemoProject.Infrastructure и сломать слои.
    // GUARDRAIL: ArchitectureRules.Api_ShouldNotReferenceInfrastructureDirectly

    public Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // In real app: read from DB via Select() + AsNoTracking()
        var booking = new Booking
        {
            Id = id,
            CustomerName = "Demo",
            ScheduledAt = DateTime.UtcNow,
            Status = BookingStatus.Pending
        };
        return Task.FromResult<Booking?>(booking);
    }

    public Task<IReadOnlyList<Booking>> GetPendingAsync(CancellationToken ct = default)
    {
        return Task.FromResult<IReadOnlyList<Booking>>(Array.Empty<Booking>());
    }

    public Task ConfirmAsync(Guid id, CancellationToken ct = default)
    {
        // In real app: load entity with tracking, modify, save
        return Task.CompletedTask;
    }
}
