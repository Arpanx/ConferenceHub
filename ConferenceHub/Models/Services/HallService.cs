namespace ConferenceHub.Models.Services
{
    public class HallService
    {
        public int HallId { get; set; }

        public Hall Hall { get; set; } = null!;

        public int ServiceId { get; set; }

        public Service Service { get; set; } = null!;
    }
}
