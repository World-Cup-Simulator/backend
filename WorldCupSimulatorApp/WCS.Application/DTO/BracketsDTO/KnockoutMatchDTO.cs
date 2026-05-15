namespace WCS.Application.DTO.BracketsDTO
{
    public class KnockoutMatchDTO
    {
        public int Key { get; set; }

        //Represents Enum Stage, 1 will be added as the tournament progresses.
        public int Stage { get; set; } = 0;
        public int NextMatchKey { get; set; }
        public int TeamAID { get; set; }
        public string TeamA { get; set; } = string.Empty;
        public double AAccumulatedScores { get; set; }
        public double AAccumulatedWeights { get; set; }
        public double AAccumulatedPenalties { get; set; }
        public int AAccumulatedCount { get; set; }
        public int? TeamBID { get; set; }
        public string TeamB { get; set; } = string.Empty;
        public double BAccumulatedScores { get; set; }
        public double BAccumulatedWeights { get; set; }
        public double BAccumulatedPenalties { get; set; }
        public int BAccumulatedCount { get; set; }
    }
}
