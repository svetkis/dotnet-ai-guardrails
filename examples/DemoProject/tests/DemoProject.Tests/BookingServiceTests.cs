using DemoProject.Application;
using DemoProject.Domain;
using TUnit;

namespace DemoProject.Tests;

public class BookingServiceTests
{
    [Test]
    public async Task GetByIdAsync_ShouldReturnBooking()
    {
        var svc = new BookingService();
        var id = BookingId.New();
        var result = await svc.GetByIdAsync(id);

        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Id).IsEqualTo(id);
    }

    [Test]
    public async Task GetPendingAsync_ShouldReturnEmptyList()
    {
        var svc = new BookingService();
        var result = await svc.GetPendingAsync();

        await Assert.That(result).IsNotNull();
        await Assert.That(result.Count).IsEqualTo(0);
    }
}
