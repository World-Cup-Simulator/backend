using WCS.Application.DTO.RatingsDTO;

namespace WCS.Application.Services.Ratings
{
    public interface IRatingService
    {
        AttackRatingDTO CalculateAttack(List<RatingDataDTO> data, double accumulatedScores, double accumulatedWeights);
        DefenseRatingDTO CalculateDefense(List<RatingDataDTO> data, double accumulatedPenalties, int accumulatedCount);

    }
}
