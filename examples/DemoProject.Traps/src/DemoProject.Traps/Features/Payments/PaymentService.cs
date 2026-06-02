using DemoProject.Traps.Features.Orders;

namespace DemoProject.Traps.Features.Payments;

// TRAP: Агент добавил using из соседней фичи "ради одного DTO".
// GUARDRAIL: Slice().NotHaveDependenciesBetweenSlices() ловит межмодульную зависимость.
public class PaymentService
{
    public void ProcessPayment(OrderDto order)
    {
        ArgumentNullException.ThrowIfNull(order);
    }
}
