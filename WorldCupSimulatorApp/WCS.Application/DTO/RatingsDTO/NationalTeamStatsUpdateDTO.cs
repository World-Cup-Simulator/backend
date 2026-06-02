namespace WCS.Application.DTO.RatingsDTO
{
    // DTO for bulk updating NationalTeam accumulated statistics.
    public class NationalTeamStatsUpdateDTO
    {
        public int TeamId { get; set; }
        public double AttackRating { get; set; }
        public double AccumulatedScores { get; set; }
        public double AccumulatedWeights { get; set; }
        public double DefenseRating { get; set; }
        public double AccumulatedPenalties { get; set; }
        public int AccumulatedCount { get; set; }
    }
}
