using WCS.Domain.Enums;

namespace WCS.Application.DTO.UpdatesDTO
{
    public class HistoricalMatchUpdateDTO
    {
        public DateOnly Date { get; set; }
        public int GoalsA { get; set; }
        public int GoalsB { get; set; }
        public Competition Competition { get; set; }
        public Stage Stage { get; set; }
        public int TeamAId { get; set; }
        public int TeamBId { get; set; }
    }
}
