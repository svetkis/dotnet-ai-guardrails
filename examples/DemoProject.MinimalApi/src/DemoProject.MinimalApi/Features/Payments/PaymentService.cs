using DemoProject.MinimalApi.Domain;

namespace DemoProject.MinimalApi.Features.Payments;

public class PaymentService
{
    private static readonly List<Payment> _payments = new();

    public Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var payment = _payments.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(payment);
    }

    public Task<IReadOnlyList<Payment>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        var payments = _payments.Where(p => p.OrderId == orderId).ToList();
        return Task.FromResult<IReadOnlyList<Payment>>(payments);
    }

    public Task<Payment> CreateAsync(Guid orderId, decimal amount, CancellationToken ct = default)
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Amount = amount,
            Status = PaymentStatus.Pending
        };

        _payments.Add(payment);
        return Task.FromResult(payment);
    }

    public Task<bool> CompleteAsync(Guid id, CancellationToken ct = default)
    {
        var payment = _payments.FirstOrDefault(p => p.Id == id);
        if (payment is null)
            return Task.FromResult(false);

        _payments.Remove(payment);
        _payments.Add(payment with { Status = PaymentStatus.Completed });
        return Task.FromResult(true);
    }
}
