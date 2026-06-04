using DemoProject.Traps.Modules.Orders;

namespace DemoProject.Traps.Modules.Shipping;

// TRAP: Замыкание цикла — Shipping зависит от Orders.
// GUARDRAIL: ArchUnitNET BeFreeOfCycles ловит этот цикл.
// NetArchTest.NotHaveDependenciesBetweenSlices тоже словил бы,
// но запретил бы ВСЕ межмодульные зависимости, даже легальные DAG.
public class ShippingService
{
    public void Ship(IOrderRepository order) { }
}
