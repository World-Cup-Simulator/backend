using WCS.Application.DTO.RatingsDTO;
using WCS.Domain.Entities;

namespace WCS.Application.Services.Ratings
{
    public class AttackRatingCalculator
    {
        public static AttackRatingDTO Calculate(List<RatingDataDTO> data, RatingWeightsOptions weights, 
            double accumulatedScores, double accumulatedWeights)
        {
            if (data == null || data.Count == 0) return new AttackRatingDTO
            {
                AttackRating = accumulatedWeights <= 0 ? 0 : accumulatedScores / accumulatedWeights,
                AccumulatedScores = accumulatedScores <= 0 ? 0 : accumulatedScores,
                AccumulatedWeights = accumulatedWeights <= 0 ? 0 : accumulatedWeights
            };

            double sumWeights = 0;
            double sumScores = 0;

            if (accumulatedScores > 0 && accumulatedWeights > 0)
            {
                sumWeights = accumulatedWeights;
                sumScores = accumulatedScores;
            }

            foreach (var match in data)
            {
                double score = 0;
                double recencyWeight = RatingHelper.GetRecencyWeight(match.Date);
                double rivalWeight = RatingHelper.GetRankingWeight(match.OpponentFifaRank);

                // Try to get the value, if it fails or is null, use 0.85
                double competitionWeight = 0.85;
                if (weights?.Competition != null && weights.Competition.TryGetValue(match.Competition.ToString(), out var compW))
                {
                    competitionWeight = compW;
                }

                // Try to get the value, if it fails or is null, use 1.0
                double stageWeight = 1.0;
                if (weights?.Stage != null && weights.Stage.TryGetValue(match.Stage.ToString(), out var stageW))
                {
                    stageWeight = stageW;
                }

                // Combined importance of the match.
                double totalWeight = recencyWeight * rivalWeight * competitionWeight * stageWeight;

                // Offensive contribution adjusted by match importance.
                score = match.GoalsScored * totalWeight;

                sumScores += score;
                sumWeights += totalWeight;
            }

            // Weighted average goals scored.
            var attackRating = sumWeights == 0 ? 0 : sumScores / sumWeights;

            return new AttackRatingDTO
            {
                AttackRating = attackRating,
                AccumulatedScores = sumScores,
                AccumulatedWeights = sumWeights,
            };
        }
    }
}
