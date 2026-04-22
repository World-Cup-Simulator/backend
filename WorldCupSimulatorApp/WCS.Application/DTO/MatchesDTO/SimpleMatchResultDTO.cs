using WCS.Domain.Enums;

namespace WCS.Application.DTO.MatchesDTO
{
    public class SimpleMatchResultDTO
    {
        public string TeamA { get; set; } = string.Empty;
        public string TeamB { get; set; } = string.Empty;
        public MatchOutcome Winner { get; set; }
        public int? WinnerID { get; set; }
        public double OutcomeProbability { get; set; }
    }
}
