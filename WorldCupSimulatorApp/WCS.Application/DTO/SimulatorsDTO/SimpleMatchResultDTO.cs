using WCS.Domain.Enums;

namespace WCS.Application.DTO.SimulatorsDTO
{
    public class SimpleMatchResultDTO : IMatchResult
    {
        public string TeamA { get; set; } = string.Empty;
        public string TeamB { get; set; } = string.Empty;
        public MatchOutcome Winner { get; set; }
        public DateOnly Date { get; set; }
        public int TeamAID { get; set; }
        public int TeamBID { get; set; }
        public double OutcomeProbability { get; set; }
    }
}
