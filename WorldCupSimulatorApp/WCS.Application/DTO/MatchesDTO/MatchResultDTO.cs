using WCS.Domain.Enums;

namespace WCS.Application.DTO.MatchesDTO
{
    public class MatchResultDTO
    {
        public string TeamA { get; set; } = string.Empty;
        public string TeamB { get; set; } = string.Empty;
        public int GoalsA { get; set; }
        public int GoalsB { get; set; }
        public MatchOutcome Winner { get; set; }
        public int TeamAID { get; set; }
        public int TeamBID { get; set; }
        public double OutcomeProbability { get; set; }
        public double ScoreProbability { get; set; }
        public bool DecidedByPenalties { get; set; } = false;
    }
}
