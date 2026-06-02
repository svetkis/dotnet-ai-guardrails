namespace DemoProject.Domain;

public interface IBookingService
{
    Task<Booking?> GetByIdAsync(BookingId id, CancellationToken ct = default);
    Task<IReadOnlyList<Booking>> GetPendingAsync(CancellationToken ct = default);
    Task ConfirmAsync(BookingId id, CancellationToken ct = default);
}
