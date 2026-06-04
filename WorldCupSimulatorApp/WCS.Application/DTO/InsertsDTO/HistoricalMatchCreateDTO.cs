using WCS.Domain.Enums;

namespace WCS.Application.DTO.InsertsDTO
{
    public class HistoricalMatchCreateDTO
    {
        public DateOnly Date { get; set; }
        public int GoalsA { get; set; }
        public int GoalsB { get; set; }
        public Competition Competition { get; set; } = Competition.WorldCup;
        public Stage Stage { get; set; }
        public int TeamAId { get; set; }
        public int TeamBId { get; set; }
    }
}
