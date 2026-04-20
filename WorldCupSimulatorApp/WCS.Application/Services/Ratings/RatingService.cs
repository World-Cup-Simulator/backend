using Microsoft.Extensions.Options;
using WCS.Application.DTO.RatingsDTO;
using WCS.Domain.Entities;

namespace WCS.Application.Services.Ratings
{
    public class RatingService(IOptions<RatingWeightsOptions> options) : IRatingService
    {
        private readonly RatingWeightsOptions _RatingWeights = options.Value;

        public double CalculateAttack (List<RatingDataDTO> data)
        {
            return AttackRatingCalculator.Calculate(data, _RatingWeights);
        }

        public double CalculateDefense(List<RatingDataDTO> data)
        {
            return DefenseRatingCalculator.Calculate(data, _RatingWeights);
        }
    }
}
