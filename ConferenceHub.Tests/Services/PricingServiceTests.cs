using ConferenceHub.Exceptions;
using ConferenceHub.Services;

namespace ConferenceHub.Tests.Services;

public class PricingServiceTests
{
    private readonly PricingService _pricingService = new();

    [Fact]
    public void CalculateRoomCost_StandardHours_ReturnsBasePrice()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 7, 10, 0, 0);
        var endTime = new DateTime(2026, 9, 7, 12, 0, 0);

        // Act
        var result = _pricingService.CalculateRoomCost(
            2000m,
            startTime,
            endTime);

        // Assert
        Assert.Equal(4000m, result);
    }

    [Fact]
    public void CalculateRoomCost_MorningDiscount_Applies10PercentDiscount()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 7, 6, 0, 0);
        var endTime = new DateTime(2026, 9, 7, 8, 0, 0);

        // Act
        var result = _pricingService.CalculateRoomCost(
            2000m,
            startTime,
            endTime);

        // Assert
        Assert.Equal(3600m, result);
    }

    [Fact]
    public void CalculateRoomCost_EveningDiscount_Applies20PercentDiscount()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 7, 18, 0, 0);
        var endTime = new DateTime(2026, 9, 7, 20, 0, 0);

        // Act
        var result = _pricingService.CalculateRoomCost(
            2000m,
            startTime,
            endTime);

        // Assert
        Assert.Equal(3200m, result);
    }

    [Fact]
    public void CalculateRoomCost_PeakHours_Applies15PercentIncrease()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 7, 12, 0, 0);
        var endTime = new DateTime(2026, 9, 7, 14, 0, 0);

        // Act
        var result = _pricingService.CalculateRoomCost(
            2000m,
            startTime,
            endTime);

        // Assert
        Assert.Equal(4600m, result);
    }

    [Fact]
    public void CalculateRoomCost_CrossesTariffPeriods_CalculatesEachPeriodSeparately()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 7, 11, 0, 0);
        var endTime = new DateTime(2026, 9, 7, 15, 0, 0);

        // Act
        var result = _pricingService.CalculateRoomCost(
            2000m,
            startTime,
            endTime);

        // Assert
        Assert.Equal(8600m, result);
    }

    [Fact]
    public void CalculateRoomCost_InvalidTimeRange_ThrowsException()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 7, 12, 0, 0);
        var endTime = new DateTime(2026, 9, 7, 10, 0, 0);

        // Act & Assert
        Assert.Throws<BusinessException>(() =>
            _pricingService.CalculateRoomCost(
                2000m,
                startTime,
                endTime));
    }
}