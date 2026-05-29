namespace WCS.Application.DTO.MatchesDTO
{
    public interface IScoreResult : IMatchResult
    {
        public int GoalsA { get; set; }
        public int GoalsB { get; set; }
        public double ScoreProbability { get; set; }
        public bool DecidedByPenalties { get; set; }
    }
}
