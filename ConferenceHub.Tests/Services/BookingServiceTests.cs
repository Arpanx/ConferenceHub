using ConferenceHub.Data;
using ConferenceHub.Exceptions;
using ConferenceHub.Models;
using ConferenceHub.Models.Dtos;
using ConferenceHub.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHub.Tests.Services;

public class BookingServiceTests
{
    private static ConferenceHubDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ConferenceHubDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ConferenceHubDbContext(options);
    }

    private static async Task SeedAsync(
        ConferenceHubDbContext context)
    {
        var projector = new AdditionalService
        {
            Id = 1,
            Name = "Проєктор",
            Price = 500m,
            IsActive = true
        };

        var wiFi = new AdditionalService
        {
            Id = 2,
            Name = "Wi-Fi",
            Price = 300m,
            IsActive = true
        };

        var sound = new AdditionalService
        {
            Id = 3,
            Name = "Звук",
            Price = 700m,
            IsActive = true
        };

        var inactiveService = new AdditionalService
        {
            Id = 4,
            Name = "Неактивная услуга",
            Price = 1000m,
            IsActive = false
        };

        context.AdditionalServices.AddRange(
            projector,
            wiFi,
            sound,
            inactiveService);

        var hallA = new Hall
        {
            Id = 1,
            Name = "Зал А",
            Capacity = 50,
            BaseHourlyRate = 2000m,
            IsActive = true
        };

        var hallB = new Hall
        {
            Id = 2,
            Name = "Зал B",
            Capacity = 100,
            BaseHourlyRate = 3500m,
            IsActive = true
        };

        var inactiveHall = new Hall
        {
            Id = 3,
            Name = "Неактивный зал",
            Capacity = 50,
            BaseHourlyRate = 1500m,
            IsActive = false
        };

        context.Halls.AddRange(
            hallA,
            hallB,
            inactiveHall);

        context.HallServices.AddRange(
            new HallAdditionalService
            {
                HallId = 1,
                AdditionalServiceId = 1
            },
            new HallAdditionalService
            {
                HallId = 1,
                AdditionalServiceId = 2
            },
            new HallAdditionalService
            {
                HallId = 1,
                AdditionalServiceId = 3
            },
            new HallAdditionalService
            {
                HallId = 1,
                AdditionalServiceId = 4
            },
            new HallAdditionalService
            {
                HallId = 2,
                AdditionalServiceId = 1
            },
            new HallAdditionalService
            {
                HallId = 2,
                AdditionalServiceId = 2
            });

        await context.SaveChangesAsync();
    }

    private static BookingService CreateService(
        ConferenceHubDbContext context)
    {
        return new BookingService(
            context,
            new PricingService());
    }

    [Fact]
    public async Task CreateAsync_ValidBooking_CreatesBooking()
    {
        await using var context = CreateContext();
        await SeedAsync(context);

        var service = CreateService(context);

        var startTime = new DateTime(
            2026, 9, 7, 10, 0, 0);

        var model = new CreateBookingDto
        {
            HallId = 1,
            StartTime = startTime,
            Duration = TimeSpan.FromHours(2),
            ServiceIds = []
        };

        var result = await service.CreateAsync(model);

        Assert.NotEqual(0, result.Id);
        Assert.Equal(1, result.HallId);
        Assert.Equal("Зал А", result.HallName);
        Assert.Equal(startTime, result.StartTime);
        Assert.Equal(
            startTime.AddHours(2),
            result.EndTime);

        Assert.Equal(4000m, result.RoomCost);
        Assert.Equal(0m, result.ServicesCost);
        Assert.Equal(4000m, result.TotalCost);
    }

    [Fact]
    public async Task CreateAsync_WithServices_CalculatesTotalCost()
    {
        await using var context = CreateContext();
        await SeedAsync(context);

        var service = CreateService(context);

        var model = new CreateBookingDto
        {
            HallId = 1,
            StartTime = new DateTime(
                2026, 9, 7, 10, 0, 0),
            Duration = TimeSpan.FromHours(2),
            ServiceIds = [1, 2]
        };

        var result = await service.CreateAsync(model);

        Assert.Equal(4000m, result.RoomCost);
        Assert.Equal(800m, result.ServicesCost);
        Assert.Equal(4800m, result.TotalCost);

        Assert.Equal(2, result.Services.Count);
        Assert.Contains(result.Services, x => x.Id == 1);
        Assert.Contains(result.Services, x => x.Id == 2);
    }

    [Fact]
    public async Task CreateAsync_SavesBookingToDatabase()
    {
        await using var context = CreateContext();
        await SeedAsync(context);

        var service = CreateService(context);

        var model = new CreateBookingDto
        {
            HallId = 1,
            StartTime = new DateTime(
                2026, 9, 7, 10, 0, 0),
            Duration = TimeSpan.FromHours(2),
            ServiceIds = [1, 2]
        };

        var result = await service.CreateAsync(model);

        var booking = await context.Bookings
            .Include(x => x.BookingServices)
            .FirstOrDefaultAsync(x => x.Id == result.Id);

        Assert.NotNull(booking);

        Assert.Equal(1, booking.HallId);
        Assert.Equal(4800m, booking.TotalCost);

        Assert.Equal(
            2,
            booking.BookingServices.Count);

        Assert.Contains(
            booking.BookingServices,
            x => x.AdditionalServiceId == 1 && x.Price == 500m);

        Assert.Contains(
            booking.BookingServices,
            x => x.AdditionalServiceId == 2 && x.Price == 300m);
    }

    [Fact]
    public async Task CreateAsync_NonExistingHall_ThrowsBusinessException()
    {
        await using var context = CreateContext();
        await SeedAsync(context);

        var service = CreateService(context);

        var model = new CreateBookingDto
        {
            HallId = 999,
            StartTime = new DateTime(
                2026, 9, 7, 10, 0, 0),
            Duration = TimeSpan.FromHours(2)
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.CreateAsync(model));

        Assert.Equal(
            "HALL_NOT_FOUND",
            exception.ErrorCode);

        Assert.Equal(
            StatusCodes.Status404NotFound,
            exception.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_InactiveHall_ThrowsBusinessException()
    {
        await using var context = CreateContext();
        await SeedAsync(context);

        var service = CreateService(context);

        var model = new CreateBookingDto
        {
            HallId = 3,
            StartTime = new DateTime(
                2026, 9, 7, 10, 0, 0),
            Duration = TimeSpan.FromHours(2)
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.CreateAsync(model));

        Assert.Equal(
            "HALL_NOT_ACTIVE",
            exception.ErrorCode);

        Assert.Equal(
            StatusCodes.Status409Conflict,
            exception.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_InvalidDuration_ThrowsBusinessException()
    {
        await using var context = CreateContext();
        await SeedAsync(context);

        var service = CreateService(context);

        var model = new CreateBookingDto
        {
            HallId = 1,
            StartTime = new DateTime(
                2026, 9, 7, 10, 0, 0),
            Duration = TimeSpan.Zero
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.CreateAsync(model));

        Assert.Equal(
            "INVALID_DURATION",
            exception.ErrorCode);
    }

    [Fact]
    public async Task CreateAsync_OverlappingBooking_ThrowsBusinessException()
    {
        await using var context = CreateContext();
        await SeedAsync(context);

        context.Bookings.Add(new Booking
        {
            Id = 1,
            HallId = 1,
            StartTime = new DateTime(
                2026, 9, 7, 10, 0, 0),
            EndTime = new DateTime(
                2026, 9, 7, 12, 0, 0),
            TotalCost = 4000m
        });

        await context.SaveChangesAsync();

        var service = CreateService(context);

        var model = new CreateBookingDto
        {
            HallId = 1,
            StartTime = new DateTime(
                2026, 9, 7, 11, 0, 0),
            Duration = TimeSpan.FromHours(2)
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.CreateAsync(model));

        Assert.Equal(
            "HALL_ALREADY_BOOKED",
            exception.ErrorCode);

        Assert.Equal(
            StatusCodes.Status409Conflict,
            exception.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_NonOverlappingBooking_CreatesBooking()
    {
        await using var context = CreateContext();
        await SeedAsync(context);

        context.Bookings.Add(new Booking
        {
            Id = 1,
            HallId = 1,
            StartTime = new DateTime(
                2026, 9, 7, 10, 0, 0),
            EndTime = new DateTime(
                2026, 9, 7, 12, 0, 0),
            TotalCost = 4000m
        });

        await context.SaveChangesAsync();

        var service = CreateService(context);

        var model = new CreateBookingDto
        {
            HallId = 1,
            StartTime = new DateTime(
                2026, 9, 7, 12, 0, 0),
            Duration = TimeSpan.FromHours(2)
        };

        var result = await service.CreateAsync(model);

        Assert.NotEqual(0, result.Id);
        Assert.Equal(1, result.HallId);
    }

    [Fact]
    public async Task CreateAsync_ServiceNotAvailable_ThrowsBusinessException()
    {
        await using var context = CreateContext();
        await SeedAsync(context);

        var service = CreateService(context);

        var model = new CreateBookingDto
        {
            HallId = 2,
            StartTime = new DateTime(
                2026, 9, 7, 10, 0, 0),
            Duration = TimeSpan.FromHours(2),
            ServiceIds = [3]
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.CreateAsync(model));

        Assert.Equal(
            "SERVICE_NOT_AVAILABLE",
            exception.ErrorCode);
    }

    [Fact]
    public async Task CreateAsync_InactiveService_ThrowsBusinessException()
    {
        await using var context = CreateContext();
        await SeedAsync(context);

        var service = CreateService(context);

        var model = new CreateBookingDto
        {
            HallId = 1,
            StartTime = new DateTime(
                2026, 9, 7, 10, 0, 0),
            Duration = TimeSpan.FromHours(2),
            ServiceIds = [4]
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.CreateAsync(model));

        Assert.Equal(
            "SERVICE_NOT_AVAILABLE",
            exception.ErrorCode);
    }

    [Fact]
    public async Task CreateAsync_DuplicateServiceIds_CountsServiceOnlyOnce()
    {
        await using var context = CreateContext();
        await SeedAsync(context);

        var service = CreateService(context);

        var model = new CreateBookingDto
        {
            HallId = 1,
            StartTime = new DateTime(
                2026, 9, 7, 10, 0, 0),
            Duration = TimeSpan.FromHours(2),
            ServiceIds = [1, 1, 2, 2]
        };

        var result = await service.CreateAsync(model);

        Assert.Equal(800m, result.ServicesCost);
        Assert.Equal(4800m, result.TotalCost);

        Assert.Equal(2, result.Services.Count);
    }

    [Fact]
    public async Task CreateAsync_EveningBooking_AppliesEveningDiscount()
    {
        await using var context = CreateContext();
        await SeedAsync(context);

        var service = CreateService(context);

        var model = new CreateBookingDto
        {
            HallId = 1,
            StartTime = new DateTime(
                2026, 9, 7, 18, 0, 0),
            Duration = TimeSpan.FromHours(2)
        };

        var result = await service.CreateAsync(model);

        Assert.Equal(3200m, result.RoomCost);
        Assert.Equal(3200m, result.TotalCost);
    }

    [Fact]
    public async Task CreateAsync_PeakBooking_AppliesPeakPrice()
    {
        await using var context = CreateContext();
        await SeedAsync(context);

        var service = CreateService(context);

        var model = new CreateBookingDto
        {
            HallId = 1,
            StartTime = new DateTime(
                2026, 9, 7, 12, 0, 0),
            Duration = TimeSpan.FromHours(2)
        };

        var result = await service.CreateAsync(model);

        Assert.Equal(4600m, result.RoomCost);
        Assert.Equal(4600m, result.TotalCost);
    }
}