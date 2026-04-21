using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WCS.Domain.Enums;

namespace WCS.Domain.Entities
{
    public class NationalTeam
    {
        public int NationalTeamId { get; set; }

        [MaxLength(50, ErrorMessage = "Name must not exceed 50 characters")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(3, ErrorMessage = "Code must not exceed 3 characters")]
        public string Code { get; set; } = string.Empty;
        public Confederation Confederation { get; set; }
        public int CurrentFifaRank { get; set; }
        public double AttackRating { get; set; }
        public double DefenseRating { get; set; }
        public double AccumulatedScores { get; set; }
        public double AccumulatedWeights { get; set; }
        public double AccumulatedPenalties { get; set; }
        public double AccumulatedCount { get; set; }

        public List<HistoricalMatch> TeamAMatches { get; set; } = [];
        public List<HistoricalMatch> TeamBMatches { get; set; } = [];

        public WorldCupTeam? WorldCupTeam { get; set; }
    }
}
