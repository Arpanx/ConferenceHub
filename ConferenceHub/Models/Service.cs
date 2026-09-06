namespace ConferenceHub.Models
{
    public class Service
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<HallService> HallServices { get; set; } = new List<HallService>();

        public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
    }
}
