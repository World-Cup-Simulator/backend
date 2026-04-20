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
            var calculator = new AttackRatingCalculator();
            return calculator.Calculate(data, _RatingWeights);
        }

        public double CalculateDefense(List<RatingDataDTO> data)
        {
            var calculator = new DefenseRatingCalculator();
            return calculator.Calculate(data, _RatingWeights);
        }
    }
}
