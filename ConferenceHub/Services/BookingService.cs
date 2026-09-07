using ConferenceHub.Data;
using ConferenceHub.Exceptions;
using ConferenceHub.Models;
using ConferenceHub.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHub.Services;

public class BookingService : IBookingAdditionaService
{
    private readonly ConferenceHubDbContext _context;
    private readonly IPricingService _pricingService;

    public BookingService(
        ConferenceHubDbContext context,
        IPricingService pricingService)
    {
        _context = context;
        _pricingService = pricingService;
    }

    public async Task<BookingDto> CreateAsync(
        CreateBookingDto model,
        CancellationToken cancellationToken = default)
    {
        // -------------------------
        // 1. Validate time
        // -------------------------

        if (model.Duration <= TimeSpan.Zero)
        {
            throw new BusinessException(
                "Booking duration must be greater than zero.",
                "INVALID_DURATION");
        }

        var endTime = model.StartTime + model.Duration;

        if (endTime <= model.StartTime)
        {
            throw new BusinessException(
                "Booking end time must be greater than start time.",
                "INVALID_TIME_RANGE");
        }

        // -------------------------
        // 2. Load hall
        // -------------------------

        var hall = await _context.Halls
            .Include(x => x.HallServices)
            .ThenInclude(x => x.Service)
            .FirstOrDefaultAsync(
                x => x.Id == model.HallId,
                cancellationToken);

        if (hall is null)
        {
            throw new BusinessException(
                "Hall was not found.",
                "HALL_NOT_FOUND",
                StatusCodes.Status404NotFound);
        }

        if (!hall.IsActive)
        {
            throw new BusinessException(
                "Hall is not active.",
                "HALL_NOT_ACTIVE",
                StatusCodes.Status409Conflict);
        }

        // -------------------------
        // 3. Check booking conflict
        // -------------------------

        var hasConflict = await _context.Bookings
            .AnyAsync(
                x =>
                    x.HallId == model.HallId &&
                    x.StartTime < endTime &&
                    x.EndTime > model.StartTime,
                cancellationToken);

        if (hasConflict)
        {
            throw new BusinessException(
                "The hall is already booked for the selected time.",
                "HALL_ALREADY_BOOKED",
                StatusCodes.Status409Conflict);
        }

        // -------------------------
        // 4. Validate services
        // -------------------------

        var requestedServiceIds = model.ServiceIds
            .Distinct()
            .ToList();

        var availableServices = hall.HallServices
            .Where(x =>
                x.Service.IsActive &&
                requestedServiceIds.Contains(x.ServiceId))
            .Select(x => x.Service)
            .ToList();

        if (availableServices.Count != requestedServiceIds.Count)
        {
            throw new BusinessException(
                "One or more selected services are not available for this hall.",
                "SERVICE_NOT_AVAILABLE");
        }

        // -------------------------
        // 5. Calculate room cost
        // -------------------------

        var roomCost = _pricingService.CalculateRoomCost(
            hall.BaseHourlyRate,
            model.StartTime,
            endTime);

        // -------------------------
        // 6. Calculate services
        // -------------------------

        var servicesCost = availableServices
            .Sum(x => x.Price);

        var totalCost = roomCost + servicesCost;

        // -------------------------
        // 7. Create booking
        // -------------------------

        var booking = new Booking
        {
            HallId = hall.Id,
            StartTime = model.StartTime,
            EndTime = endTime,
            TotalCost = totalCost
        };

        foreach (var service in availableServices)
        {
            booking.BookingServices.Add(
                new ConferenceHub.Models.BookingAdditionaService
                {
                    ServiceId = service.Id,
                    Price = service.Price
                });
        }

        _context.Bookings.Add(booking);

        await _context.SaveChangesAsync(cancellationToken);

        return new BookingDto
        {
            Id = booking.Id,
            HallId = hall.Id,
            HallName = hall.Name,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            RoomCost = roomCost,
            ServicesCost = servicesCost,
            TotalCost = totalCost,

            Services = availableServices
                .Select(x => new ServiceDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Price = x.Price
                })
                .ToList()
        };
    }
}