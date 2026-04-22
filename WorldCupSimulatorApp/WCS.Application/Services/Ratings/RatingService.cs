using Microsoft.Extensions.Options;
using WCS.Application.DTO.RatingsDTO;
using WCS.Domain.Entities;

namespace WCS.Application.Services.Ratings
{
    public class RatingService(IOptions<RatingWeightsOptions> options) : IRatingService
    {
        private readonly RatingWeightsOptions _RatingWeights = options.Value;

        public AttackRatingDTO CalculateHistoricalAttack (List<RatingDataDTO> data)
        {
            return AttackRatingCalculator.CalculateHistorical(data, _RatingWeights);
        }

        public AttackRatingDTO CalculateAttack(List<RatingDataDTO> data, double accumulatedScores, double accumulatedWeights)
        {
            return AttackRatingCalculator.Calculate(data, _RatingWeights, accumulatedScores, accumulatedWeights);
        }

        public DefenseRatingDTO CalculateHistoricalDefense(List<RatingDataDTO> data)
        {
            return DefenseRatingCalculator.CalculateHistorical(data, _RatingWeights);
        }

        public DefenseRatingDTO CalculateDefense(List<RatingDataDTO> data, double accumulatedPenalties, int accumulatedCount)
        {
            return DefenseRatingCalculator.Calculate(data, _RatingWeights, accumulatedPenalties, accumulatedCount);
        }
    }
}
