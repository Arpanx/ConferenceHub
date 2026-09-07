namespace ConferenceHub.Models
{
    public class BookingAdditionaService
    {
        public int BookingId { get; set; }

        public Booking Booking { get; set; } = null!;

        public int ServiceId { get; set; }

        public AdditionalService Service { get; set; } = null!;

        public decimal Price { get; set; }
    }
}
