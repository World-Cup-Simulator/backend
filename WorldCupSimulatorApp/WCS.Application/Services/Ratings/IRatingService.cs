using WCS.Application.DTO.RatingsDTO;

namespace WCS.Application.Services.Ratings
{
    public interface IRatingService
    {
        AttackRatingDTO CalculateHistoricalAttack(List<RatingDataDTO> data);
        AttackRatingDTO CalculateAttack(List<RatingDataDTO> data, double accumulatedScores, double accumulatedWeights);
        DefenseRatingDTO CalculateHistoricalDefense(List<RatingDataDTO> data);
        DefenseRatingDTO CalculateDefense(List<RatingDataDTO> data, double accumulatedPenalties, int accumulatedCount);

    }
}
