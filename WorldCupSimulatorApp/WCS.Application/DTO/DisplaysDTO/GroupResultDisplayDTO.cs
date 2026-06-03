using WCS.Domain.Enums;

namespace WCS.Application.DTO.DisplaysDTO
{
    // Simplified group result for frontend display.
    public class GroupResultDisplayDTO
    {
        public string GroupCode { get; set; } = string.Empty;
        public string TeamA { get; set; } = string.Empty;
        public string TeamB { get; set; } = string.Empty;
        public int GoalsA { get; set; }
        public int GoalsB { get; set; }
        public MatchOutcome Winner { get; set; }
        public DateOnly Date { get; set; }
        public double OutcomeProbability { get; set; }
        public double? ScoreProbability { get; set; }
        public bool? DecidedByPenalties { get; set; }
    }
}
