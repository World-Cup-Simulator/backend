using Microsoft.Extensions.Options;
using WCS.Application.DTO.RatingsDTO;
using WCS.Domain.Entities;

namespace WCS.Application.Services.Ratings
{
    public class RatingService(IOptions<RatingWeightsOptions> options) : IRatingService
    {
        private readonly RatingWeightsOptions _RatingWeights = options.Value;

        public AttackRatingDTO CalculateAttack(List<RatingDataDTO> data, double accumulatedScores = 0, double accumulatedWeights = 0)
        {
            return AttackRatingCalculator.Calculate(data, _RatingWeights, accumulatedScores, accumulatedWeights);
        }

        public DefenseRatingDTO CalculateDefense(List<RatingDataDTO> data, double accumulatedPenalties = 0, int accumulatedCount = 0)
        {
            return DefenseRatingCalculator.Calculate(data, _RatingWeights, accumulatedPenalties, accumulatedCount);
        }
    }
}
