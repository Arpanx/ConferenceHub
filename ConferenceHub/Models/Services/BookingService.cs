namespace ConferenceHub.Models.Services
{
    public class BookingService
    {
        public int BookingId { get; set; }

        public Booking Booking { get; set; } = null!;

        public int ServiceId { get; set; }

        public Service Service { get; set; } = null!;

        public decimal Price { get; set; }
    }
}
