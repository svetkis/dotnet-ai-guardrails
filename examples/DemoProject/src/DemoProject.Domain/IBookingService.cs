namespace DemoProject.Domain;

public interface IBookingService
{
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Booking>> GetPendingAsync(CancellationToken ct = default);
    Task ConfirmAsync(Guid id, CancellationToken ct = default);
}
