namespace ConferenceHub.Models.Dtos
{
    public class CreateHallDto
    {
        public string Name { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public decimal BaseHourlyRate { get; set; }

        public List<int> ServiceIds { get; set; } = [];
    }
}
