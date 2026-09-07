using ConferenceHub.Exceptions;

namespace ConferenceHub.Services;

public class PricingService : IPricingService
{
    public decimal CalculateRoomCost(
        decimal hourlyRate,
        DateTime startTime,
        DateTime endTime)
    {
        if (endTime <= startTime)
        {
            throw new BusinessException(
                "Booking end time must be greater than start time.",
                "INVALID_TIME_RANGE");
        }

        decimal total = 0m;

        var current = startTime;

        while (current < endTime)
        {
            var nextBoundary = GetNextBoundary(current, endTime);

            var durationHours =
                (decimal)(nextBoundary - current).TotalHours;

            var multiplier = GetTariffMultiplier(current);

            total += hourlyRate * durationHours * multiplier;

            current = nextBoundary;
        }

        return Math.Round(total, 2);
    }

    private static decimal GetTariffMultiplier(DateTime time)
    {
        var currentTime = time.TimeOfDay;

        // 12:00–14:00 — peak +15%.
        if (currentTime >= TimeSpan.FromHours(12) &&
            currentTime < TimeSpan.FromHours(14))
        {
            return 1.15m;
        }

        // 06:00–09:00 — discount 10%.
        if (currentTime >= TimeSpan.FromHours(6) &&
            currentTime < TimeSpan.FromHours(9))
        {
            return 0.90m;
        }

        // 18:00–23:00 — discount 20%.
        if (currentTime >= TimeSpan.FromHours(18) &&
            currentTime < TimeSpan.FromHours(23))
        {
            return 0.80m;
        }

        // 09:00–18:00 — standard price.
        return 1.00m;
    }

    private static DateTime GetNextBoundary(
        DateTime current,
        DateTime endTime)
    {
        var boundaries = new[]
        {
            current.Date.AddHours(6),
            current.Date.AddHours(9),
            current.Date.AddHours(12),
            current.Date.AddHours(14),
            current.Date.AddHours(18),
            current.Date.AddHours(23),
            current.Date.AddDays(1)
        };

        return boundaries
            .Where(x => x > current)
            .DefaultIfEmpty(endTime)
            .Min() < endTime
                ? boundaries
                    .Where(x => x > current && x < endTime)
                    .DefaultIfEmpty(endTime)
                    .Min()
                : endTime;
    }
}