namespace DemoProject.Domain;

public sealed record Booking
{
    public Guid Id { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public DateTime ScheduledAt { get; init; }
    public BookingStatus Status { get; init; }
}

public enum BookingStatus
{
    Pending,
    Confirmed,
    Cancelled
}
