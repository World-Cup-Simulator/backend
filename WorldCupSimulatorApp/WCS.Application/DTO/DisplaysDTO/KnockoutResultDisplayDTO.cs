using WCS.Domain.Enums;

namespace WCS.Application.DTO.DisplaysDTO
{
    /// <summary>
    /// Simplified knockout result for frontend display.
    /// Includes probabilities but strips accumulated stats.
    /// </summary>
    public class KnockoutResultDisplayDTO
    {
        public string TeamA { get; set; } = string.Empty;
        public string TeamB { get; set; } = string.Empty;
        public int GoalsA { get; set; }
        public int GoalsB { get; set; }
        public MatchOutcome Winner { get; set; }
        public double OutcomeProbability { get; set; }
        public double? ScoreProbability { get; set; }
        public bool? DecidedByPenalties { get; set; }
    }
}
