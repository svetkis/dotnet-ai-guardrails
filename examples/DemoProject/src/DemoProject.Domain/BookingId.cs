using System.Text.Json.Serialization;

namespace DemoProject.Domain;

// TRAP: Агент передаёт Guid клиента в метод, ожидающий Guid агента — компилятор молчит.
// GUARDRAIL: BookingId — это не Guid. Метод GetByIdAsync(BookingId) не примет CustomerId.
// Ошибка ловится за секунды при наборе кода, без запуска тестов.
[JsonConverter(typeof(BookingIdJsonConverter))]
public readonly record struct BookingId(Guid Value)
{
    public static BookingId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}
