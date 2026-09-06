namespace ConferenceHub.Models
{
    public class Booking
    {
        public int Id { get; set; }

        public int HallId { get; set; }

        public Hall Hall { get; set; } = null!;

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public decimal TotalCost { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
    }
}
