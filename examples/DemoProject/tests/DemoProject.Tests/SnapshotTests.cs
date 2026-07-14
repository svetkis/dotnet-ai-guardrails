// GUARDRAIL: OpenAPI snapshot тест ловит любое изменение API контракта.
// Этот файл — рабочая адаптация шаблона из tests/patterns/SnapshotTest.cs
// Для демонстрации используем JSON-сериализацию доменной модели вместо HTTP.

using System.Text.Json;
using DemoProject.Domain;
using TUnit;

namespace DemoProject.Tests;

public class SnapshotTests
{
    private static readonly string SnapshotDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Snapshots");
    private static readonly string SnapshotPath = Path.Combine(SnapshotDir, "booking-snapshot.json");

    [Test]
    public async Task BookingDto_ShouldMatchSnapshot()
    {
        var booking = new Booking
        {
            Id = new BookingId(Guid.Parse("11111111-1111-1111-1111-111111111111")),
            CustomerId = new CustomerId(Guid.Parse("22222222-2222-2222-2222-222222222222")),
            CustomerName = "Demo Customer",
            ScheduledAt = new DateTime(2026, 5, 30, 10, 0, 0, DateTimeKind.Utc),
            Status = BookingStatus.Confirmed
        };

        var currentJson = JsonSerializer.Serialize(booking, new JsonSerializerOptions { WriteIndented = true });

        if (!File.Exists(SnapshotPath))
        {
            Directory.CreateDirectory(SnapshotDir);
            await File.WriteAllTextAsync(SnapshotPath, currentJson);
            await Assert.That(File.Exists(SnapshotPath)).IsTrue()
                .Because("Snapshot file must be created on first run.");
            return;
        }

        var snapshot = await File.ReadAllTextAsync(SnapshotPath);
        await Assert.That(NormalizeJson(currentJson)).IsEqualTo(NormalizeJson(snapshot))
            .Because("Any DTO change must be intentional. Update snapshot after front-end review.");
    }

    [Test]
    public async Task DateTime_ShouldHaveUtcMarker()
    {
        var dto = new Booking
        {
            Id = BookingId.New(),
            ScheduledAt = DateTime.UtcNow,
            Status = BookingStatus.Pending
        };

        var json = JsonSerializer.Serialize(dto);

        await Assert.That(json).Contains("Z")
            .Because("UTC dates must serialize with 'Z' suffix to avoid timezone bugs.");
    }

    private static string NormalizeJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
    }
}
