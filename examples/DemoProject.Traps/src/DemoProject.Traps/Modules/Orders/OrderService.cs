using DemoProject.Traps.Modules.Payments;

namespace DemoProject.Traps.Modules.Orders;

// TRAP: Агент добавил зависимость на Payments "ради одного вызова".
// Это начало цикла: Orders -> Payments -> Shipping -> Orders.
// NetArchTest.NotHaveDependenciesBetweenSlices словил бы и это тоже,
// но запретил бы ВСЕ межмодульные зависимости — даже легальные DAG.
public class OrderService
{
    public void CreateOrder(IPaymentGateway payment) { }
}
