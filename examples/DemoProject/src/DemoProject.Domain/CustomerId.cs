using System.Text.Json.Serialization;

namespace DemoProject.Domain;

// TRAP: Агент путает идентификаторы разных сущностей, потому что все они Guid.
// GUARDRAIL: CustomerId и BookingId — разные типы. Компилятор не даст подставить один вместо другого.
[JsonConverter(typeof(CustomerIdJsonConverter))]
public readonly record struct CustomerId(Guid Value)
{
    public static CustomerId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}
