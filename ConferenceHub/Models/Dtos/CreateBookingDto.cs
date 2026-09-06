namespace ConferenceHub.Models.Dtos;

public class CreateBookingDto
{
    public int HallId { get; set; }

    public DateTime StartTime { get; set; }

    public TimeSpan Duration { get; set; }

    public List<int> ServiceIds { get; set; } = [];
}