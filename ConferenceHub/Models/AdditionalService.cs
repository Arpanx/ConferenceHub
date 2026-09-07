namespace ConferenceHub.Models
{
    public class AdditionalService
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<HallAdditionalService> HallServices { get; set; } = new List<HallAdditionalService>();

        public ICollection<BookingAdditionalService> BookingServices { get; set; } = new List<BookingAdditionalService>();
    }
}
