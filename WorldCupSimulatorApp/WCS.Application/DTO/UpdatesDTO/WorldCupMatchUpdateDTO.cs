namespace WCS.Application.DTO.UpdatesDTO
{
    // DTO for bulk updating WorldCupMatch score.
    public class WorldCupMatchUpdateDTO
    {
        public int WorldCupMatchId { get; set; }
        public int? GoalsA { get; set; }
        public int? GoalsB { get; set; }
    }
}
