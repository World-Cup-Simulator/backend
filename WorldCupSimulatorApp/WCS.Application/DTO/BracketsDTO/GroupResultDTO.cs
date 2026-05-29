using WCS.Domain.Enums;

namespace WCS.Application.DTO.BracketsDTO
{
    public class GroupResultDTO
    {
        public string GroupCode {  get; set; } = string.Empty;
        public string TeamA { get; set; } = string.Empty;
        public string TeamB { get; set; } = string.Empty;
        public int GoalsA { get; set; }
        public int GoalsB { get; set; }
        public int TeamAFifaRank { get; set; }
        public int TeamBFifaRank { get; set; }
        public double AAttackRating { get; set; }
        public double BAttackRating { get; set; }
        public MatchOutcome Winner { get; set; }
        public DateOnly Date { get; set; }
        public int TeamAID { get; set; }
        public int TeamBID { get; set; }
        public double OutcomeProbability { get; set; }
        public double? ScoreProbability { get; set; }
        public bool? DecidedByPenalties { get; set; } = false;
    }
}
