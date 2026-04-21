using WCS.Application.DTO.RatingsDTO;
using WCS.Domain.Entities;

namespace WCS.Application.Services.Ratings
{
    public class AttackRatingCalculator
    {
        public static AttackRatingDTO CalculateHistorical(List<RatingDataDTO> data, RatingWeightsOptions weights)
        {
            if (data == null || data.Count == 0) return new AttackRatingDTO();

            var helper = new RatingHelper();

            double sumWeights = 0;
            double sumScores = 0;

            foreach (var match in data)
            {
                double score = 0;
                double recencyWeight = helper.GetRecencyWeight(match.Date);
                double rivalWeight = helper.GetRankingWeight(match.OpponentFifaRank);

                if (!weights.Competition.TryGetValue(
                    match.Competition.ToString(),
                    out var competitionWeight))
                {
                    competitionWeight = 0.85;
                }

                if (!weights.Stage.TryGetValue(
                    match.Stage.ToString(),
                    out var stageWeight))
                {
                    stageWeight = 1;
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

        public static double Calculate(List<RatingDataDTO> data, RatingWeightsOptions weights, double accumulatedScores,
            double accumulatedWeights)
        {
            if (data == null || data.Count == 0) return 0;

            var helper = new RatingHelper();

            double sumWeights = accumulatedWeights;
            double sumScores = accumulatedScores;

            foreach (var match in data)
            {
                double score = 0;
                double recencyWeight = helper.GetRecencyWeight(match.Date);
                double rivalWeight = helper.GetRankingWeight(match.OpponentFifaRank);

                if (!weights.Competition.TryGetValue(
                    match.Competition.ToString(),
                    out var competitionWeight))
                {
                    competitionWeight = 0.85;
                }

                if (!weights.Stage.TryGetValue(
                    match.Stage.ToString(),
                    out var stageWeight))
                {
                    stageWeight = 1;
                }

                // Combined importance of the match.
                double totalWeight = recencyWeight * rivalWeight * competitionWeight * stageWeight;

                // Offensive contribution adjusted by match importance.
                score = match.GoalsScored * totalWeight;

                sumScores += score;
                sumWeights += totalWeight;
            }

            // Weighted average goals scored.
            return sumWeights == 0 ? 0 : sumScores / sumWeights;
        }
    }
}
