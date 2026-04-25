namespace WCS.Application.DTO.MatchesDTO
{
    public class SimpleSimulationMatchDTO
    {
        public int TeamAID { get; set; }
        public string TeamA { get; set; } = string.Empty;        
        public double AAttackRating { get; set; }
        public double ADefenseRating { get; set; }
        public int TeamBID { get; set; }
        public string TeamB { get; set; } = string.Empty;
        public double BAttackRating { get; set; }
        public double BDefenseRating { get; set; }
    }
}
