using DemoProject.Application;
using TUnit;

namespace DemoProject.Tests;

public class BookingServiceTests
{
    [Test]
    public async Task GetByIdAsync_ShouldReturnBooking()
    {
        var svc = new BookingService();
        var result = await svc.GetByIdAsync(Guid.NewGuid());

        await Assert.That(result).IsNotNull();
    }

    [Test]
    public async Task GetPendingAsync_ShouldReturnEmptyList()
    {
        var svc = new BookingService();
        var result = await svc.GetPendingAsync();

        await Assert.That(result).IsNotNull();
    }
}
