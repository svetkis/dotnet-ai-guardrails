using DemoProject.MinimalApi.Domain;

namespace DemoProject.MinimalApi.Features.Payments;

public static class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payments");

        group.MapGet("/order/{orderId:guid}", async (Guid orderId, PaymentService service, CancellationToken ct) =>
            Results.Ok(await service.GetByOrderIdAsync(orderId, ct)));

        group.MapGet("/{id:guid}", async (Guid id, PaymentService service, CancellationToken ct) =>
        {
            var payment = await service.GetByIdAsync(id, ct);
            return payment is null ? Results.NotFound() : Results.Ok(payment);
        });

        group.MapPost("/", async (CreatePaymentRequest request, PaymentService service, CancellationToken ct) =>
        {
            var payment = await service.CreateAsync(request.OrderId, request.Amount, ct);
            return Results.Created($"/api/payments/{payment.Id}", payment);
        });

        group.MapPost("/{id:guid}/complete", async (Guid id, PaymentService service, CancellationToken ct) =>
        {
            var success = await service.CompleteAsync(id, ct);
            return success ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }
}

public record CreatePaymentRequest(Guid OrderId, decimal Amount);
