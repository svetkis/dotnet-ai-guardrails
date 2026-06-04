using DemoProject.MinimalApi.Features.Orders;
using DemoProject.MinimalApi.Features.Payments;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// TRAP: Агент мог забыть маппинг endpoint'ов при рефакторинге.
// GUARDRAIL: Integration-тесты проверяют, что endpoint отвечает.
app.MapOrderEndpoints();
app.MapPaymentEndpoints();

app.Run();

// Для интеграционных тестов
public partial class Program { }
