namespace ConferenceHub.Services;

public interface IPricingService
{
    decimal CalculateRoomCost(
        decimal hourlyRate,
        DateTime startTime,
        DateTime endTime);
}