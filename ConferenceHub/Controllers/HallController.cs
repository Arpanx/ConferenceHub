using ConferenceHub.Models.Dtos;
using ConferenceHub.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceHub.Controllers;

/// <summary>
/// Керування конференц-залами.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HallController : ControllerBase
{
    private readonly IHallAdditionalService _hallService;

    public HallController(IHallAdditionalService hallService)
    {
        _hallService = hallService;
    }

    /// <summary>
    /// Отримати список усіх активних конференц-залів.
    /// </summary>
    /// <returns>Список конференц-залів.</returns>
    /// <response code="200">Список залів успішно отримано.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<HallDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<HallDto>>> GetHalls(
        CancellationToken cancellationToken)
    {
        var halls = await _hallService.GetAllAsync(cancellationToken);

        return Ok(halls);
    }

    /// <summary>
    /// Отримати конференц-зал за ідентифікатором.
    /// </summary>
    /// <param name="id">Ідентифікатор залу.</param>
    /// <returns>Інформація про конференц-зал.</returns>
    /// <response code="200">Зал знайдено.</response>
    /// <response code="404">Зал із вказаним ідентифікатором не знайдено.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(HallDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Отримати список послуг, доступних у конференц-залі.
    /// </summary>
    /// <param name="id">Ідентифікатор залу.</param>
    /// <returns>Список доступних послуг.</returns>
    /// <response code="200">Список послуг успішно отримано.</response>
    /// <response code="404">Зал не знайдено.</response>
    [HttpGet("{id:int}/services")]
    [ProducesResponseType(typeof(List<ServiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Створити новий конференц-зал.
    /// </summary>
    /// <param name="model">Дані нового конференц-залу.</param>
    /// <returns>Створений конференц-зал.</returns>
    /// <response code="201">Зал успішно створено.</response>
    /// <response code="400">Передано некоректні дані.</response>
    [HttpPost]
    [ProducesResponseType(typeof(HallDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

    /// <summary>
    /// Змінити існуючий конференц-зал.
    /// </summary>
    /// <param name="id">Ідентифікатор залу.</param>
    /// <param name="model">Нові дані залу.</param>
    /// <returns>Оновлений конференц-зал.</returns>
    /// <response code="200">Зал успішно оновлено.</response>
    /// <response code="400">Передано некоректні дані.</response>
    /// <response code="404">Зал не знайдено.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(HallDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Деактивувати конференц-зал.
    /// </summary>
    /// <param name="id">Ідентифікатор залу.</param>
    /// <response code="204">Зал успішно деактивовано.</response>
    /// <response code="404">Зал не знайдено.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Знайти вільні конференц-зали,
    /// що відповідають необхідній місткості та вказаному часу.
    /// </summary>
    /// <param name="startTime">Дата та час початку бронювання.</param>
    /// <param name="duration">Тривалість бронювання.</param>
    /// <param name="capacity">Мінімально необхідна місткість залу.</param>
    /// <returns>Список доступних конференц-залів.</returns>
    /// <response code="200">Список доступних залів успішно отримано.</response>
    /// <response code="400">Передано некоректні параметри пошуку.</response>
    [HttpGet("available")]
    [ProducesResponseType(typeof(List<HallDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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