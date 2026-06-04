using DemoProject.Traps.Modules.Shipping;

namespace DemoProject.Traps.Modules.Payments;

// TRAP: Продолжение цикла — Payments зависит от Shipping.
public class PaymentService
{
    public void Pay(IShippingProvider shipping) { }
}
