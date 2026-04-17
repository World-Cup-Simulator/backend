using System.ComponentModel.DataAnnotations;
using WCS.Domain.Enums;

namespace WCS.Domain.Entities
{
    public class HistoricalMatch
    {
        public int HistoricalMatchId { get; set; }

        public DateOnly Date { get; set; }

        [Range(0, 20, ErrorMessage = "Goals must be between 0 and 20")]
        public int GoalsA { get; set; }

        [Range(0, 20, ErrorMessage = "Goals must be between 0 and 20")]
        public int GoalsB { get; set; }
        public Competition Competition { get; set; }
        public Stage Stage { get; set; }
        public int TeamAId { get; set; }
        public int TeamBId { get; set; }

        public NationalTeam TeamA { get; set; } = null!;
        public NationalTeam TeamB { get; set; } = null!;
    }
}
