namespace ConferenceHub.Models;

public class HallAdditionalService
{
    public int HallId { get; set; }

    public Hall Hall { get; set; } = null!;

    public int AdditionalServiceId { get; set; }

    public AdditionalService AdditionalService { get; set; } = null!;
}