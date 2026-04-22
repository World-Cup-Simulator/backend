namespace WCS.Application.DTO.ProbabilitiesDTO
{
    public class MatchProbabilityDTO
    {
        public List<ScoreProbabilityDTO> Scores { get; set; } = [];
        public double WinA { get; set; }
        public double Draw { get; set; }
        public double WinB { get; set; }
    }
}
