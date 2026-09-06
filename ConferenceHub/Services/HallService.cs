using ConferenceHub.Data;
using ConferenceHub.Exceptions;
using ConferenceHub.Models;
using ConferenceHub.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHub.Services;

public class HallService : IHallService
{
    private readonly ConferenceHubDbContext _context;

    public HallService(ConferenceHubDbContext context)
    {
        _context = context;
    }

    public async Task<List<HallDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Halls
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new HallDto
            {
                Id = x.Id,
                Name = x.Name,
                Capacity = x.Capacity,
                BaseHourlyRate = x.BaseHourlyRate,
                IsActive = x.IsActive,

                Services = x.HallServices
                    .Where(hs => hs.Service.IsActive)
                    .Select(hs => new ServiceDto
                    {
                        Id = hs.Service.Id,
                        Name = hs.Service.Name,
                        Price = hs.Service.Price
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<HallDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Halls
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new HallDto
            {
                Id = x.Id,
                Name = x.Name,
                Capacity = x.Capacity,
                BaseHourlyRate = x.BaseHourlyRate,
                IsActive = x.IsActive,

                Services = x.HallServices
                    .Where(hs => hs.Service.IsActive)
                    .Select(hs => new ServiceDto
                    {
                        Id = hs.Service.Id,
                        Name = hs.Service.Name,
                        Price = hs.Service.Price
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<ServiceDto>?> GetServicesAsync(
        int hallId,
        CancellationToken cancellationToken = default)
    {
        var hallExists = await _context.Halls
            .AnyAsync(x => x.Id == hallId, cancellationToken);

        if (!hallExists)
        {
            return null;
        }

        return await _context.HallServices
            .AsNoTracking()
            .Where(x =>
                x.HallId == hallId &&
                x.Service.IsActive)
            .Select(x => new ServiceDto
            {
                Id = x.Service.Id,
                Name = x.Service.Name,
                Price = x.Service.Price
            })
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<HallDto> CreateAsync(
        CreateHallDto model,
        CancellationToken cancellationToken = default)
    {
        ValidateHall(model.Name, model.Capacity, model.BaseHourlyRate);

        var serviceIds = model.ServiceIds
            .Distinct()
            .ToList();

        var services = await GetActiveServicesAsync(
            serviceIds,
            cancellationToken);

        if (services.Count != serviceIds.Count)
        {
            throw new BusinessException(
                "One or more services do not exist or are inactive.",
                "INVALID_SERVICES");
        }

        var hall = new Hall
        {
            Name = model.Name.Trim(),
            Capacity = model.Capacity,
            BaseHourlyRate = model.BaseHourlyRate,
            IsActive = true
        };

        _context.Halls.Add(hall);

        await _context.SaveChangesAsync(cancellationToken);

        foreach (var service in services)
        {
            _context.HallServices.Add(new ConferenceHub.Models.HallService
            {
                HallId = hall.Id,
                ServiceId = service.Id
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(hall.Id, cancellationToken)
               ?? throw new InvalidOperationException(
                   "The hall was created but could not be loaded.");
    }

    public async Task<HallDto?> UpdateAsync(
        int id,
        UpdateHallDto model,
        CancellationToken cancellationToken = default)
    {
        ValidateHall(model.Name, model.Capacity, model.BaseHourlyRate);

        var hall = await _context.Halls
            .Include(x => x.HallServices)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (hall is null)
        {
            return null;
        }

        var serviceIds = model.ServiceIds
            .Distinct()
            .ToList();

        var services = await GetActiveServicesAsync(
            serviceIds,
            cancellationToken);

        if (services.Count != serviceIds.Count)
        {
            throw new ArgumentException(
                "One or more services do not exist or are inactive.");
        }

        hall.Name = model.Name.Trim();
        hall.Capacity = model.Capacity;
        hall.BaseHourlyRate = model.BaseHourlyRate;
        hall.IsActive = model.IsActive;

        _context.HallServices.RemoveRange(hall.HallServices);

        foreach (var service in services)
        {
            _context.HallServices.Add(new ConferenceHub.Models.HallService
            {
                HallId = hall.Id,
                ServiceId = service.Id
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var hall = await _context.Halls
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (hall is null)
        {
            return false;
        }

        // Soft delete.
        hall.IsActive = false;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task<List<Service>> GetActiveServicesAsync(
        List<int> serviceIds,
        CancellationToken cancellationToken)
    {
        if (serviceIds.Count == 0)
        {
            return [];
        }

        return await _context.Services
            .Where(x =>
                serviceIds.Contains(x.Id) &&
                x.IsActive)
            .ToListAsync(cancellationToken);
    }

    private static void ValidateHall(
        string name,
        int capacity,
        decimal baseHourlyRate)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BusinessException(
                "Hall name is required.",
                "INVALID_HALL_NAME");
        }

        if (capacity <= 0)
        {
            throw new BusinessException(
               "Capacity must be greater than zero.",
               "INVALID_HALL_CAPACITY");
        }

        if (baseHourlyRate < 0)
        {
            throw new BusinessException(
                "Base hourly rate cannot be negative.",
                "INVALID_HALL_PRICE");
        }
    }
}