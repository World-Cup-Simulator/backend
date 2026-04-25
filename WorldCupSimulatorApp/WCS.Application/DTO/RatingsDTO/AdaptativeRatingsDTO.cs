namespace WCS.Application.DTO.RatingsDTO
{
    public class AdaptativeRatingsDTO
    {
        public double LambdaA { get; set; }
        public double LambdaB { get; set; }
        public AttackRatingDTO AAttackRating { get; set; } = new AttackRatingDTO();
        public DefenseRatingDTO ADefenseRating { get; set; } = new DefenseRatingDTO();
        public AttackRatingDTO BAttackRating { get; set; } = new AttackRatingDTO();
        public DefenseRatingDTO BDefenseRating { get; set; } = new DefenseRatingDTO();
    }
}
