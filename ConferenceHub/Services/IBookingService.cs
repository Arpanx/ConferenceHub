using ConferenceHub.Models.Dtos;

namespace ConferenceHub.Services;

public interface IBookingService
{
    Task<BookingDto> CreateAsync(
        CreateBookingDto model,
        CancellationToken cancellationToken = default);
}