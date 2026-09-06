using ConferenceHub.Models.Dtos;
using ConferenceHub.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceHub.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HallController : ControllerBase
{
    private readonly IHallService _hallService;

    public HallController(IHallService hallService)
    {
        _hallService = hallService;
    }

    // GET: api/hall
    [HttpGet]
    public async Task<ActionResult<List<HallDto>>> GetHalls(
        CancellationToken cancellationToken)
    {
        var halls = await _hallService.GetAllAsync(cancellationToken);

        return Ok(halls);
    }

    // GET: api/hall/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<HallDto>> GetHall(
        int id,
        CancellationToken cancellationToken)
    {
        var hall = await _hallService.GetByIdAsync(
            id,
            cancellationToken);

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
        var services = await _hallService.GetServicesAsync(
            id,
            cancellationToken);

        if (services is null)
        {
            return NotFound();
        }

        return Ok(services);
    }

    // POST: api/hall
    [HttpPost]
    public async Task<ActionResult<HallDto>> CreateHall(
        [FromBody] CreateHallDto model,
        CancellationToken cancellationToken)
    {
        var hall = await _hallService.CreateAsync(
            model,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetHall),
            new { id = hall.Id },
            hall);
    }

    // PUT: api/hall/1
    [HttpPut("{id:int}")]
    public async Task<ActionResult<HallDto>> UpdateHall(
        int id,
        [FromBody] UpdateHallDto model,
        CancellationToken cancellationToken)
    {
        var hall = await _hallService.UpdateAsync(
            id,
            model,
            cancellationToken);

        if (hall is null)
        {
            return NotFound();
        }

        return Ok(hall);
    }

    // DELETE: api/hall/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteHall(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted = await _hallService.DeleteAsync(
            id,
            cancellationToken);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("available")]
    public async Task<ActionResult<List<HallDto>>> GetAvailableHalls(
    [FromQuery] DateTime startTime,
    [FromQuery] TimeSpan duration,
    [FromQuery] int capacity,
    CancellationToken cancellationToken)
    {
        var halls = await _hallService.GetAvailableAsync(
            startTime,
            duration,
            capacity,
            cancellationToken);

        return Ok(halls);
    }
}