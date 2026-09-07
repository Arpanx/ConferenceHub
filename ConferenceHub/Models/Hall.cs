namespace ConferenceHub.Models
{
    public class Hall
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public decimal BaseHourlyRate { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<HallAdditionalService> HallServices { get; set; } = new List<HallAdditionalService>();

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
