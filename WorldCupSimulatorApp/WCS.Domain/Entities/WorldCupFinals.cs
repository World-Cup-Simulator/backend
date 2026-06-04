using System.ComponentModel.DataAnnotations;
using WCS.Domain.Enums;

namespace WCS.Domain.Entities
{
    public class WorldCupFinals
    {
        public int WorldCupFinalsId { get; set; }

        [Range(1, 16, ErrorMessage = "Key must be between 1 and 16")]
        public int Key { get; set; }
		public Stage Stage { get; set; }
        public DateOnly Date { get; set; }

        [Range(1, 8, ErrorMessage = "Key must be between 1 and 8")]
        public int NextMatchKey { get; set; }
		public int TeamAId { get; set; }
		public int TeamBId { get; set; }
        public bool Played { get; set; } = false;
        public int? GoalsA { get; set; }
        public int? GoalsB { get; set; }
        public WorldCupTeam TeamA { get; set; } = null!;
        public WorldCupTeam TeamB { get; set; } = null!;
    }
}
