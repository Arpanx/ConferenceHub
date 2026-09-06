namespace ConferenceHub.Models.Dtos;

public class BookingDto
{
    public int Id { get; set; }

    public int HallId { get; set; }

    public string HallName { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public decimal RoomCost { get; set; }

    public decimal ServicesCost { get; set; }

    public decimal TotalCost { get; set; }

    public List<ServiceDto> Services { get; set; } = [];
}