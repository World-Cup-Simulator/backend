namespace WCS.Application.DTO.MatchesDTO
{
    public class AdaptativeSimulationMatchDTO
    {
        public int TeamAID { get; set; }
        public string TeamA { get; set; } = string.Empty;
        public double AAccumulatedScores { get; set; }
        public double AAccumulatedWeights { get; set; }
        public double AAccumulatedPenalties { get; set; }
        public int AAccumulatedCount { get; set; }
        public int TeamBID { get; set; }
        public string TeamB { get; set; } = string.Empty;
        public double BAccumulatedScores { get; set; }
        public double BAccumulatedWeights { get; set; }
        public double BAccumulatedPenalties { get; set; }
        public int BAccumulatedCount { get; set; }
    }
}
