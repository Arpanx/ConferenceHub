namespace ConferenceHub.Models
{
    public class Hall
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public decimal BaseHourlyRate { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<HallService> HallServices { get; set; } = new List<HallService>();

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
