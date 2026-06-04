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

        [Range(0, 9, ErrorMessage = "Points must be between 0 and 9")]
        public int Points { get; set; }

        public int TeamId { get; set; }

        public NationalTeam Team { get; set; } = null!;

        public List<WorldCupMatch> TeamAMatches { get; set; } = [];
        public List<WorldCupMatch> TeamBMatches { get; set; } = [];
        public List<WorldCupFinals> TeamAFinalsMatches { get; set; } = [];
        public List<WorldCupFinals> TeamBFinalsMatches { get; set; } = [];
    }
}
