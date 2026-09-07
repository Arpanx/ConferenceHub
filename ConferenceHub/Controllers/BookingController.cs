using ConferenceHub.Models;
using ConferenceHub.Models.Dtos;
using ConferenceHub.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceHub.Controllers;

/// <summary>
/// Керування бронюваннями конференц-залів.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BookingController : ControllerBase
{
    private readonly IBookingAdditionaService _bookingService;

    public BookingController(IBookingAdditionaService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>
    /// Створити нове бронювання конференц-залу.
    /// </summary>
    /// <param name="model">Дані для створення бронювання.</param>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <returns>Створене бронювання з інформацією про вартість.</returns>
    /// <response code="200">Бронювання успішно створено.</response>
    /// <response code="400">Передано некоректні дані.</response>
    /// <response code="404">Конференц-зал не знайдено.</response>
    /// <response code="409">Зал недоступний на вказаний час або неактивний.</response>
    [HttpPost]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingDto>> CreateBooking(
        [FromBody] CreateBookingDto model,
        CancellationToken cancellationToken)
    {
        var booking = await _bookingService.CreateAsync(
            model,
            cancellationToken);

        return Ok(booking);
    }
}