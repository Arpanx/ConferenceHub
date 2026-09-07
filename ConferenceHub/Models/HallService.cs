namespace ConferenceHub.Models
{
    public class HallService
    {
        public int HallId { get; set; }

        public Hall Hall { get; set; } = null!;

        public int ServiceId { get; set; }

        public AdditionalService Service { get; set; } = null!;
    }
}
