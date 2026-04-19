using Microsoft.Extensions.Options;
using WCS.Application.DTO.RatingsDTO;
using WCS.Domain.Entities;

namespace WCS.Application.Services.Ratings
{
    public class AttackRatingCalculator
    {
        private readonly RatingWeightsOptions _RatingWeights;

        public AttackRatingCalculator(IOptions<RatingWeightsOptions> options)
        {
            _RatingWeights = options.Value;
        }

        public double Calculate(List<RatingDataDTO> data)
        {
            if (data == null || data.Count == 0) return 0;

            var helper = new RatingHelper();

            double sumWeights = 0;
            double sumScores = 0;

            foreach (var match in data)
            {
                double score = 0;
                double recencyWeight = helper.GetRecencyWeight(match.Date);
                double rivalWeight = helper.GetRankingWeight(match.OpponentFifaRank);

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

                score = match.GoalsScored * totalWeight;

                sumScores += score;
                sumWeights += totalWeight;
            }

            return sumWeights == 0 ? 0 : sumScores / sumWeights;
        }
    }
}
