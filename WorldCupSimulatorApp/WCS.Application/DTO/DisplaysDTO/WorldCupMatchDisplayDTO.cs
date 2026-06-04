namespace WCS.Application.DTO.DisplaysDTO
{
    // Lightweight DTO for displaying World Cup match information in the frontend.
    public class WorldCupMatchDisplayDTO
    {
        public int MatchId { get; set; }
        public int Round { get; set; }
        public DateOnly Date { get; set; }
        public string GroupCode { get; set; } = string.Empty;
        public string TeamAName { get; set; } = string.Empty;
        public string TeamBName { get; set; } = string.Empty;
        public string? TeamACode { get; set; }  // 3-letter code (ARG, BRA, etc.)
        public string? TeamBCode { get; set; }
        public int? GoalsA { get; set; }
        public int? GoalsB { get; set; }
    }
}
