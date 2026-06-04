namespace DemoProject.MinimalApi.Domain;

// TRAP: Агент добавляет public setters, ломая инварианты.
// GUARDRAIL: record с init-only properties — immutable by default.
public record Payment
{
    public required Guid Id { get; init; }
    public required Guid OrderId { get; init; }
    public required decimal Amount { get; init; }
    public required PaymentStatus Status { get; init; }
}

public enum PaymentStatus
{
    Pending,
    Completed,
    Failed,
    Refunded
}
