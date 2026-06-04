using WCS.Domain.Enums;

namespace WCS.Application.DTO.DisplaysDTO
{
    // Lightweight DTO for displaying World Cup finals match information in the frontend.
    public class WorldCupFinalsDisplayDTO
    {
        public int MatchId { get; set; }
        public int Key { get; set; }
        public Stage Stage { get; set; }
        public DateOnly Date { get; set; }
        public int NextMatchKey { get; set; }
        public string TeamAName { get; set; } = string.Empty;
        public string TeamBName { get; set; } = string.Empty;
        public string? TeamACode { get; set; }  // 3-letter code (ARG, BRA, etc.)
        public string? TeamBCode { get; set; }
        public int? GoalsA { get; set; }
        public int? GoalsB { get; set; }
    }
}
