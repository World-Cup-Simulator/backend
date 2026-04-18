using System.ComponentModel.DataAnnotations;

namespace WCS.Domain.Entities
{
    public class WorldCupMatch
    {
        public int WorldCupMatchId { get; set; }

        [Range(1, 3, ErrorMessage = "Round must be between 1 and 3")]
        public int Round { get; set; }
        public DateOnly Date { get; set; }

        [MaxLength(1, ErrorMessage = "Code must not exceed 1 character")]
        public string GroupCode { get; set; } = string.Empty;
        public int TeamAId { get; set; }
        public int TeamBId { get; set; }

        public WorldCupTeam TeamA { get; set; } = null!;
        public WorldCupTeam TeamB { get; set; } = null!;
    }
}
