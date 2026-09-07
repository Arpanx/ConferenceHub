using ConferenceHub.Data;
using ConferenceHub.Exceptions;
using ConferenceHub.Models;
using ConferenceHub.Models.Dtos;
using ConferenceHub.Services;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHub.Tests.Services;

public class HallServiceTests
{
    private static ConferenceHubDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ConferenceHubDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ConferenceHubDbContext(options);
    }

    private static async Task SeedAsync(ConferenceHubDbContext context)
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

        context.AdditionalServices.AddRange(
            projector,
            wiFi,
            sound);

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

        var hallC = new Hall
        {
            Id = 3,
            Name = "Зал C",
            Capacity = 30,
            BaseHourlyRate = 1500m,
            IsActive = true
        };

        context.Halls.AddRange(
            hallA,
            hallB,
            hallC);

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
                HallId = 2,
                AdditionalServiceId = 1
            },
            new HallAdditionalService
            {
                HallId = 2,
                AdditionalServiceId = 2
            },
            new HallAdditionalService
            {
                HallId = 2,
                AdditionalServiceId = 3
            },
            new HallAdditionalService
            {
                HallId = 3,
                AdditionalServiceId = 1
            });

        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateAsync_ValidHall_CreatesHallWithServices()
    {
        await using var context = CreateContext();
        await SeedAsync(context);

        var service = new HallService(context);

        var model = new CreateHallDto
        {
            Name = "Новий зал",
            Capacity = 80,
            BaseHourlyRate = 2500m,
            ServiceIds = [1, 2]
        };

        var result = await service.CreateAsync(model);

        Assert.NotEqual(0, result.Id);
        Assert.Equal("Новий зал", result.Name);
        Assert.Equal(80, result.Capacity);
        Assert.Equal(2500m, result.BaseHourlyRate);
        Assert.True(result.IsActive);

        Assert.Equal(2, result.Services.Count);
        Assert.Contains(result.Services, x => x.Id == 1);
        Assert.Contains(result.Services, x => x.Id == 2);
    }

    [Fact]
    public async Task CreateAsync_InvalidService_ThrowsBusinessException()
    {
        await using var context = CreateContext();
        await SeedAsync(context);

        var service = new HallService(context);

        var model = new CreateHallDto
        {
            Name = "Новий зал",
            Capacity = 80,
            BaseHourlyRate = 2500m,
            ServiceIds = [1, 999]
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.CreateAsync(model));

        Assert.Equal("INVALID_SERVICES", exception.ErrorCode);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingHall_ReturnsHall()
    {
        await using var context = CreateContext();
        await SeedAsync(context);

        var service = new HallService(context);

        var result = await service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Зал А", result.Name);
        Assert.Equal(50, result.Capacity);
        Assert.Equal(2000m, result.BaseHourlyRate);

        Assert.Equal(2, result.Services.Count);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingHall_ReturnsNull()
    {
        await using var context = CreateContext();
        await SeedAsync(context);

        var service = new HallService(context);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ValidHall_UpdatesHallAndServices()
    {
        await using var context = CreateContext();
        await SeedAsync(context);

        var service = new HallService(context);

        var model = new UpdateHallDto
        {
            Name = "Зал А Updated",
            Capacity = 60,
            BaseHourlyRate = 2200m,
            IsActive = true,
            ServiceIds = [1, 3]
        };

        var result = await service.UpdateAsync(1, model);

        Assert.NotNull(result);
        Assert.Equal("Зал А Updated", result.Name);
        Assert.Equal(60, result.Capacity);
        Assert.Equal(2200m, result.BaseHourlyRate);

        Assert.Equal(2, result.Services.Count);
        Assert.Contains(result.Services, x => x.Id == 1);
        Assert.Contains(result.Services, x => x.Id == 3);
        Assert.DoesNotContain(result.Services, x => x.Id == 2);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingHall_ReturnsNull()
    {
        await using var context = CreateContext();
        await SeedAsync(context);

        var service = new HallService(context);

        var model = new UpdateHallDto
        {
            Name = "Несуществующий зал",
            Capacity = 50,
            BaseHourlyRate = 2000m,
            IsActive = true,
            ServiceIds = []
        };

        var result = await service.UpdateAsync(999, model);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ExistingHall_SoftDeletesHall()
    {
        await using var context = CreateContext();
        await SeedAsync(context);

        var service = new HallService(context);

        var result = await service.DeleteAsync(1);

        Assert.True(result);

        var hall = await context.Halls
            .FirstAsync(x => x.Id == 1);

        Assert.False(hall.IsActive);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingHall_ReturnsFalse()
    {
        await using var context = CreateContext();
        await SeedAsync(context);

        var service = new HallService(context);

        var result = await service.DeleteAsync(999);

        Assert.False(result);
    }

    [Fact]
    public async Task GetAvailableAsync_ReturnsFreeHallsWithRequiredCapacity()
    {
        await using var context = CreateContext();
        await SeedAsync(context);

        var service = new HallService(context);

        var startTime = new DateTime(
            2026, 9, 7, 10, 0, 0);

        var result = await service.GetAvailableAsync(
            startTime,
            TimeSpan.FromHours(2),
            40);

        Assert.Equal(2, result.Count);

        Assert.Contains(result, x => x.Id == 1);
        Assert.Contains(result, x => x.Id == 2);

        Assert.DoesNotContain(result, x => x.Id == 3);
    }

    [Fact]
    public async Task GetAvailableAsync_BookedHall_DoesNotReturnBookedHall()
    {
        await using var context = CreateContext();
        await SeedAsync(context);

        context.Bookings.Add(new Booking
        {
            HallId = 1,
            StartTime = new DateTime(
                2026, 9, 7, 10, 0, 0),
            EndTime = new DateTime(
                2026, 9, 7, 12, 0, 0),
            TotalCost = 4000m
        });

        await context.SaveChangesAsync();

        var service = new HallService(context);

        var result = await service.GetAvailableAsync(
            new DateTime(
                2026, 9, 7, 11, 0, 0),
            TimeSpan.FromHours(2),
            40);

        Assert.DoesNotContain(result, x => x.Id == 1);
        Assert.Contains(result, x => x.Id == 2);
    }

    [Fact]
    public async Task GetAvailableAsync_InactiveHall_DoesNotReturnHall()
    {
        await using var context = CreateContext();
        await SeedAsync(context);

        var hall = await context.Halls
            .FirstAsync(x => x.Id == 1);

        hall.IsActive = false;

        await context.SaveChangesAsync();

        var service = new HallService(context);

        var result = await service.GetAvailableAsync(
            new DateTime(
                2026, 9, 7, 10, 0, 0),
            TimeSpan.FromHours(2),
            40);

        Assert.DoesNotContain(result, x => x.Id == 1);
    }

    [Fact]
    public async Task GetAvailableAsync_InvalidDuration_ThrowsBusinessException()
    {
        await using var context = CreateContext();
        await SeedAsync(context);

        var service = new HallService(context);

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.GetAvailableAsync(
                new DateTime(
                    2026, 9, 7, 10, 0, 0),
                TimeSpan.Zero,
                40));

        Assert.Equal("INVALID_DURATION", exception.ErrorCode);
    }

    [Fact]
    public async Task GetAvailableAsync_InvalidCapacity_ThrowsBusinessException()
    {
        await using var context = CreateContext();
        await SeedAsync(context);

        var service = new HallService(context);

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.GetAvailableAsync(
                new DateTime(
                    2026, 9, 7, 10, 0, 0),
                TimeSpan.FromHours(2),
                0));

        Assert.Equal("INVALID_CAPACITY", exception.ErrorCode);
    }
}