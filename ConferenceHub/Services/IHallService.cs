using ConferenceHub.Models.Dtos;

namespace ConferenceHub.Services;

public interface IHallService
{
    Task<List<HallDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<HallDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<List<ServiceDto>?> GetServicesAsync(
        int hallId,
        CancellationToken cancellationToken = default);

    Task<HallDto> CreateAsync(
        CreateHallDto model,
        CancellationToken cancellationToken = default);

    Task<HallDto?> UpdateAsync(
        int id,
        UpdateHallDto model,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<List<HallDto>> GetAvailableAsync(
        DateTime startTime,
        TimeSpan duration,
        int capacity,
        CancellationToken cancellationToken = default);
}