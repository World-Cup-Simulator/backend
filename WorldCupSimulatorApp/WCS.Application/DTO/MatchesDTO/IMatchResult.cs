using WCS.Domain.Enums;

namespace WCS.Application.DTO.MatchesDTO
{
    public interface IMatchResult
    {
        public string TeamA { get; set; }
        public string TeamB { get; set; }
        public MatchOutcome Winner { get; set; }
        public int TeamAID { get; set; }
        public int TeamBID { get; set; }
        public double OutcomeProbability { get; set; }        
    }
}
