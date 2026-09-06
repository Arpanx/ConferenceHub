using ConferenceHub.Models.Dtos;
using ConferenceHub.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceHub.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    // POST: api/booking
    [HttpPost]
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