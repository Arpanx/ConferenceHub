namespace ConferenceHub.Models.Dtos
{
    public class UpdateHallDto
    {
        public string Name { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public decimal BaseHourlyRate { get; set; }

        public bool IsActive { get; set; } = true;

        public List<int> ServiceIds { get; set; } = [];
    }
}
