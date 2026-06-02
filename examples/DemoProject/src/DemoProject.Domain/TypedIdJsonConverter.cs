using System.Text.Json;
using System.Text.Json.Serialization;

namespace DemoProject.Domain;

// TRAP: Агент забывает про JSON-конвертер для strongly typed ID — API начинает возвращать { "Value": "..." }.
// GUARDRAIL: SnapshotTest ловит изменение JSON-контракта. Конвертер гарантирует, что BookingId сериализуется как строка.
public class TypedIdJsonConverter<T> : JsonConverter<T> where T : struct
{
    private readonly Func<Guid, T> _factory;

    public TypedIdJsonConverter(Func<Guid, T> factory)
    {
        _factory = factory;
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var guid = reader.GetGuid();
        return _factory(guid);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var guid = (Guid?)typeof(T).GetProperty("Value")?.GetValue(value) ?? Guid.Empty;
        writer.WriteStringValue(guid);
    }
}

public class BookingIdJsonConverter : TypedIdJsonConverter<BookingId>
{
    public BookingIdJsonConverter() : base(g => new BookingId(g)) { }
}

public class CustomerIdJsonConverter : TypedIdJsonConverter<CustomerId>
{
    public CustomerIdJsonConverter() : base(g => new CustomerId(g)) { }
}
