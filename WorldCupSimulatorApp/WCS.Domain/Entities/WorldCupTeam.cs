using System.ComponentModel.DataAnnotations;

namespace WCS.Domain.Entities
{
    public class WorldCupTeam
    {
        public int WorldCupTeamId { get; set; }

        [MaxLength(1, ErrorMessage = "Code must not exceed 1 character")]
        public string GroupCode { get; set; } = string.Empty;

        [Range(1, 4, ErrorMessage = "PositionOrder must be between 1 and 4")]
        public int PositionOrder { get; set; }
        public int TeamId { get; set; }

        public NationalTeam Team { get; set; } = null!;
    }
}
