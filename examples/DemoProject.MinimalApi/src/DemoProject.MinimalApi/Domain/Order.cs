namespace DemoProject.MinimalApi.Domain;

// TRAP: Агент добавляет public setters, ломая инварианты.
// GUARDRAIL: record с init-only properties — immutable by default.
public record Order
{
    public required Guid Id { get; init; }
    public required string CustomerEmail { get; init; }
    public required decimal TotalAmount { get; init; }
    public required OrderStatus Status { get; init; }
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Shipped,
    Cancelled
}
