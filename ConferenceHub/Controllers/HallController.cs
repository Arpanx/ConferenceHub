using ConferenceHub.Data;
using ConferenceHub.Models;
using ConferenceHub.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHub.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HallController : ControllerBase
{
    private readonly ConferenceHubDbContext _context;

    public HallController(ConferenceHubDbContext context)
    {
        _context = context;
    }

    // GET: api/hall
    [HttpGet]
    public async Task<ActionResult<List<HallDto>>> GetHalls(
        CancellationToken cancellationToken)
    {
        var halls = await _context.Halls
            .AsNoTracking()
            .Include(x => x.HallServices)
                .ThenInclude(x => x.Service)
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

        return Ok(halls);
    }

    // GET: api/hall/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<HallDto>> GetHall(
        int id,
        CancellationToken cancellationToken)
    {
        var hall = await _context.Halls
            .AsNoTracking()
            .Include(x => x.HallServices)
                .ThenInclude(x => x.Service)
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

        if (hall is null)
        {
            return NotFound();
        }

        return Ok(hall);
    }

    // GET: api/hall/1/services
    [HttpGet("{id:int}/services")]
    public async Task<ActionResult<List<ServiceDto>>> GetHallServices(
        int id,
        CancellationToken cancellationToken)
    {
        var hallExists = await _context.Halls
            .AnyAsync(x => x.Id == id, cancellationToken);

        if (!hallExists)
        {
            return NotFound();
        }

        var services = await _context.HallServices
            .AsNoTracking()
            .Where(x =>
                x.HallId == id &&
                x.Service.IsActive)
            .Select(x => new ServiceDto
            {
                Id = x.Service.Id,
                Name = x.Service.Name,
                Price = x.Service.Price
            })
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return Ok(services);
    }

    // POST: api/hall
    [HttpPost]
    public async Task<ActionResult<HallDto>> CreateHall(
        [FromBody] CreateHallDto model,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            return BadRequest("Hall name is required.");
        }

        if (model.Capacity <= 0)
        {
            return BadRequest("Capacity must be greater than zero.");
        }

        if (model.BaseHourlyRate < 0)
        {
            return BadRequest("Base hourly rate cannot be negative.");
        }

        var serviceIds = model.ServiceIds
            .Distinct()
            .ToList();

        var services = await _context.Services
            .Where(x =>
                serviceIds.Contains(x.Id) &&
                x.IsActive)
            .ToListAsync(cancellationToken);

        if (services.Count != serviceIds.Count)
        {
            return BadRequest("One or more services do not exist or are inactive.");
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
            _context.HallServices.Add(new HallService
            {
                HallId = hall.Id,
                ServiceId = service.Id
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        var result = await GetHallDtoAsync(
            hall.Id,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetHall),
            new { id = hall.Id },
            result);
    }

    // PUT: api/hall/1
    [HttpPut("{id:int}")]
    public async Task<ActionResult<HallDto>> UpdateHall(
        int id,
        [FromBody] UpdateHallDto model,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            return BadRequest("Hall name is required.");
        }

        if (model.Capacity <= 0)
        {
            return BadRequest("Capacity must be greater than zero.");
        }

        if (model.BaseHourlyRate < 0)
        {
            return BadRequest("Base hourly rate cannot be negative.");
        }

        var hall = await _context.Halls
            .Include(x => x.HallServices)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (hall is null)
        {
            return NotFound();
        }

        var serviceIds = model.ServiceIds
            .Distinct()
            .ToList();

        var services = await _context.Services
            .Where(x =>
                serviceIds.Contains(x.Id) &&
                x.IsActive)
            .ToListAsync(cancellationToken);

        if (services.Count != serviceIds.Count)
        {
            return BadRequest("One or more services do not exist or are inactive.");
        }

        hall.Name = model.Name.Trim();
        hall.Capacity = model.Capacity;
        hall.BaseHourlyRate = model.BaseHourlyRate;
        hall.IsActive = model.IsActive;

        // Удаляем старые связи.
        _context.HallServices.RemoveRange(hall.HallServices);

        // Создаём новые.
        foreach (var service in services)
        {
            _context.HallServices.Add(new HallService
            {
                HallId = hall.Id,
                ServiceId = service.Id
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        var result = await GetHallDtoAsync(
            hall.Id,
            cancellationToken);

        return Ok(result);
    }

    // DELETE: api/hall/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteHall(
        int id,
        CancellationToken cancellationToken)
    {
        var hall = await _context.Halls
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (hall is null)
        {
            return NotFound();
        }

        // Пока используем soft delete.
        hall.IsActive = false;

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private async Task<HallDto?> GetHallDtoAsync(
        int id,
        CancellationToken cancellationToken)
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
}