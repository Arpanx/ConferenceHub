using System.ComponentModel.DataAnnotations;

namespace ConferenceHub.Models.Dtos
{
    public class CreateHallDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Range(1, 10000)]
        public int Capacity { get; set; }

        [Range(0.01, 1000000000)]
        public decimal BaseHourlyRate { get; set; }

        public List<int> ServiceIds { get; set; } = [];
    }
}
