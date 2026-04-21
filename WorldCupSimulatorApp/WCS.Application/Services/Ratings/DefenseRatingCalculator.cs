using WCS.Application.DTO.RatingsDTO;
using WCS.Domain.Entities;

namespace WCS.Application.Services.Ratings
{
    public class DefenseRatingCalculator
    {
        public static DefenseRatingDTO CalculateHistorical(List<RatingDataDTO> data, RatingWeightsOptions weights)
        {
            if (data == null || data.Count == 0) return new DefenseRatingDTO();

            var helper = new RatingHelper();

            double sumPenalties = 0;
            int count = 0;

            foreach (var match in data)
            {
                double recencyWeight = helper.GetRecencyWeight(match.Date);
                double rivalWeight = helper.GetRankingWeight(match.OpponentFifaRank);

                // Prevent division by zero and extreme values from weak opponents.
                var attack = Math.Max(0.25, match.OpponentAttackRating);

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

                // Defensive penalty:
                // conceding goals against weak attacks penalizes more,
                // conceding against strong attacks penalizes less.
                double penalty = match.GoalsConceded * totalWeight / attack;

                sumPenalties += penalty;
                count++;
            }

            double avgPenalty = sumPenalties/count;

            // Converts penalties into defensive rating.
            // 2.5 is the current max baseline and can be adjusted later.
            const double MaxDefenseRating = 2.5;
            var defenseRating = MaxDefenseRating / (1 + avgPenalty);

            return new DefenseRatingDTO
            {
                DefenseRating = defenseRating,
                AccumulatedPenalties = sumPenalties,
                AccumulatedCount = count
            };
        }

        public static double Calculate(List<RatingDataDTO> data, RatingWeightsOptions weights, double accumulatedPenalties,
            int accumulatedCount)
        {
            if (data == null || data.Count == 0) return 0;

            var helper = new RatingHelper();

            double sumPenalties = accumulatedPenalties;
            int count = accumulatedCount;

            foreach (var match in data)
            {
                double recencyWeight = helper.GetRecencyWeight(match.Date);
                double rivalWeight = helper.GetRankingWeight(match.OpponentFifaRank);

                // Prevent division by zero and extreme values from weak opponents.
                var attack = Math.Max(0.25, match.OpponentAttackRating);

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

                // Defensive penalty:
                // conceding goals against weak attacks penalizes more,
                // conceding against strong attacks penalizes less.
                double penalty = match.GoalsConceded * totalWeight / attack;

                sumPenalties += penalty;
                count++;
            }

            double avgPenalty = sumPenalties / count;

            // Converts penalties into defensive rating.
            // 2.5 is the current max baseline and can be adjusted later.
            const double MaxDefenseRating = 2.5;
            return MaxDefenseRating / (1 + avgPenalty);
        }
    }
}