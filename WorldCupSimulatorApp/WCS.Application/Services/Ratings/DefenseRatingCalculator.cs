using WCS.Application.DTO.RatingsDTO;
using WCS.Domain.Entities;

namespace WCS.Application.Services.Ratings
{
    public class DefenseRatingCalculator
    {
        public static DefenseRatingDTO Calculate(List<RatingDataDTO> data, RatingWeightsOptions weights,
            double accumulatedPenalties, int accumulatedCount)
        {
            // 2.5 is the current max baseline and can be adjusted later.
            const double MaxDefenseRating = 2.5;

            if (data == null || data.Count == 0) return new DefenseRatingDTO
            {
                DefenseRating = accumulatedCount <= 0 ? 0 : MaxDefenseRating / (1 + (accumulatedPenalties / accumulatedCount)),
                AccumulatedPenalties = accumulatedPenalties <= 0 ? 0 : accumulatedPenalties,
                AccumulatedCount = accumulatedCount <= 0 ? 0 : accumulatedCount
            };

            double sumPenalties = 0;
            int count = 0;

            if (accumulatedPenalties > 0 && accumulatedCount > 0)
            {
                sumPenalties = accumulatedPenalties;
                count = accumulatedCount;
            } 

            foreach (var match in data)
            {
                double recencyWeight = RatingHelper.GetRecencyWeight(match.Date);
                double rivalWeight = RatingHelper.GetRankingWeight(match.OpponentFifaRank);

                // Prevent division by zero and extreme values from weak opponents.
                var attack = Math.Max(0.25, match.OpponentAttackRating);
                                
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

                // Defensive penalty:
                // conceding goals against weak attacks penalizes more,
                // conceding against strong attacks penalizes less.
                double penalty = match.GoalsConceded * totalWeight / attack;

                sumPenalties += penalty;
                count++;
            }

            double avgPenalty = sumPenalties / count;

            // Converts penalties into defensive rating.            
            var defenseRating = MaxDefenseRating / (1 + avgPenalty);

            return new DefenseRatingDTO
            {
                DefenseRating = defenseRating,
                AccumulatedPenalties = sumPenalties,
                AccumulatedCount = count
            };
        }
    }
}