namespace WCS.Domain.Entities
{
    public class GroupTableEntry
    {
        public int TeamId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Points { get; set; }
        public int GoalsScored { get; set; }
        public int GoalsConceded { get; set; }
        public int GoalDifference => GoalsScored - GoalsConceded;
        public double AccumulatedScores { get; set; }
        public double AccumulatedWeights { get; set; }
        public double AccumulatedPenalties { get; set; }
        public int AccumulatedCount { get; set; }
    }
}
