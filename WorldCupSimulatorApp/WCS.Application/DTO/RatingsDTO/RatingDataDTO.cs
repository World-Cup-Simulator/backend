using WCS.Domain.Enums;

namespace WCS.Application.DTO.RatingsDTO
{
    public class RatingDataDTO
    {
        public int GoalsScored { get; set; }
        public int GoalsConceded { get; set; }
        public int OpponentFifaRank { get; set; }
        public double OpponentAttackRating { get; set; }
        public DateOnly Date { get; set; }
        public Competition Competition { get; set; }
        public Stage Stage { get; set; }

    }
}
