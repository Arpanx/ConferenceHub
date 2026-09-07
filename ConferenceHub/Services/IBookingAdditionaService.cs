using ConferenceHub.Models.Dtos;

namespace ConferenceHub.Services;

public interface IBookingAdditionaService
{
    Task<BookingDto> CreateAsync(
        CreateBookingDto model,
        CancellationToken cancellationToken = default);
}