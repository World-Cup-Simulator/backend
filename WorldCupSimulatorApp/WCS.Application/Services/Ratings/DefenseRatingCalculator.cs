using Microsoft.Extensions.Options;
using WCS.Application.DTO.RatingsDTO;
using WCS.Domain.Entities;

namespace WCS.Application.Services.Ratings
{
    public class DefenseRatingCalculator
    {
        private readonly RatingWeightsOptions _RatingWeights;

        public DefenseRatingCalculator(IOptions<RatingWeightsOptions> options)
        {
            _RatingWeights = options.Value;
        }

        public double Calculate(List<RatingDataDTO> data)
        {
            if (data == null || data.Count == 0) return 0;

            var helper = new RatingHelper();

            double sumPenalties = 0;
            int count = 0;

            foreach (var match in data)
            {
                double penalty = 0;
                double recencyWeight = helper.GetRecencyWeight(match.Date);
                double rivalWeight = helper.GetRankingWeight(match.OpponentFifaRank);

                var attack = Math.Max(0.25, match.OpponentAttackRating);

                if (!_RatingWeights.Competition.TryGetValue(
                    match.Competition.ToString(),
                    out var competitionWeight))
                {
                    competitionWeight = 0.85;
                }

                if (!_RatingWeights.Stage.TryGetValue(
                    match.Stage.ToString(),
                    out var stageWeight))
                {
                    stageWeight = 1;
                }

                double totalWeight = recencyWeight * rivalWeight * competitionWeight * stageWeight;

                penalty = match.GoalsConceded * totalWeight / attack;

                sumPenalties += penalty;
                count++;
            }

            double avgPenalty = sumPenalties/count;

            return 2.5 / (1 + avgPenalty);
        }
    }
}