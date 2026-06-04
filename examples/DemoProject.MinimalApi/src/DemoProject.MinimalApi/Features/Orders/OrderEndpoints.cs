using DemoProject.MinimalApi.Domain;

namespace DemoProject.MinimalApi.Features.Orders;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders");

        group.MapGet("/", async (OrderService service, CancellationToken ct) =>
            Results.Ok(await service.GetAllAsync(ct)));

        group.MapGet("/{id:guid}", async (Guid id, OrderService service, CancellationToken ct) =>
        {
            var order = await service.GetByIdAsync(id, ct);
            return order is null ? Results.NotFound() : Results.Ok(order);
        });

        group.MapPost("/", async (CreateOrderRequest request, OrderService service, CancellationToken ct) =>
        {
            var order = await service.CreateAsync(request.CustomerEmail, request.Amount, ct);
            return Results.Created($"/api/orders/{order.Id}", order);
        });

        group.MapPost("/{id:guid}/confirm", async (Guid id, OrderService service, CancellationToken ct) =>
        {
            var success = await service.ConfirmAsync(id, ct);
            return success ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }
}

public record CreateOrderRequest(string CustomerEmail, decimal Amount);
