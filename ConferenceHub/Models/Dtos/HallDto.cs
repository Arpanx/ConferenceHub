namespace ConferenceHub.Models.Dtos
{
    public class HallDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public decimal BaseHourlyRate { get; set; }

        public bool IsActive { get; set; }

        public List<ServiceDto> Services { get; set; } = [];
    }
}
