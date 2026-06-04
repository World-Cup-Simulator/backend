namespace WCS.Application.DTO.DisplaysDTO
{
    public class GroupTableTeamDisplayDTO
    {
        public string Name { get; set; } = string.Empty;
        public int Points { get; set; }
        public int GoalsScored { get; set; }
        public int GoalsConceded { get; set; }
        public int GoalDifference => GoalsScored - GoalsConceded;
    }
}
