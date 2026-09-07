using System.ComponentModel.DataAnnotations;

namespace ConferenceHub.Models.Dtos;

public class CreateBookingDto
{
    [Range(1, int.MaxValue)]
    public int HallId { get; set; }

    public DateTime StartTime { get; set; }

    public TimeSpan Duration { get; set; }

    public List<int> ServiceIds { get; set; } = [];
}