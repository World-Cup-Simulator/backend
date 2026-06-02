using WCS.Application.DTO.SimulatorsDTO;
using WCS.Application.DTO.ProbabilitiesDTO;
using WCS.Application.DTO.RatingsDTO;
using WCS.Domain.Enums;

namespace WCS.Application.Mappers
{
    public class SimulationMappers
    {
        // Builds a simple match result.
        public static SimpleMatchResultDTO SimpleBuildResult(SimulationMatchDTO match, MatchOutcome outcome, MatchProbabilityDTO matchProbability)
        {
            var result = new SimpleMatchResultDTO
            {
                TeamA = match.TeamA,
                TeamB = match.TeamB,
                TeamAID = match.TeamAID,
                TeamBID = match.TeamBID
            };

            (result.Winner, result.OutcomeProbability) = outcome switch
            {
                MatchOutcome.WinA => (MatchOutcome.WinA, matchProbability.WinA),
                MatchOutcome.WinB => (MatchOutcome.WinB, matchProbability.WinB),
                _ => (MatchOutcome.Draw, matchProbability.Draw)
            };

            return result;
        }

        // Builds a match result with scores. Winner for draws is set inline.
        public static MatchResultDTO BuildResult(SimulationMatchDTO match, ScoreProbabilityDTO score, MatchProbabilityDTO matchProbability)
        {
            var matchResult = new MatchResultDTO
            {
                TeamA = match.TeamA,
                TeamB = match.TeamB,
                TeamAID = match.TeamAID,
                TeamBID = match.TeamBID,
                GoalsA = score.GoalsA,
                GoalsB = score.GoalsB,
                ScoreProbability = score.Probability,
            };

            (matchResult.Winner, matchResult.OutcomeProbability) = score.GoalsA.CompareTo(score.GoalsB) switch
            {
                > 0 => (MatchOutcome.WinA, matchProbability.WinA),
                < 0 => (MatchOutcome.WinB, matchProbability.WinB),
                _ => (MatchOutcome.Draw, matchProbability.Draw)
            };

            return matchResult;
        }

        // Builds a new AdaptativeMatchResultDTO with winner data.
        public static AdaptativeMatchResultDTO BuildAdaptativeResult(MatchOutcome winner, int teamAID, int teamBID, double probability,
            bool decidedByPenalties, AttackRatingDTO winnerAttack, DefenseRatingDTO winnerDefense)
        {
            return new AdaptativeMatchResultDTO
            {
                Winner = winner,
                TeamAID = teamAID,
                TeamBID = teamBID,
                OutcomeProbability = probability,
                DecidedByPenalties = decidedByPenalties,
                WinnerAccumulatedScores = winnerAttack.AccumulatedScores,
                WinnerAccumulatedWeights = winnerAttack.AccumulatedWeights,
                WinnerAccumulatedPenalties = winnerDefense.AccumulatedPenalties,
                WinnerAccumulatedCount = winnerDefense.AccumulatedCount
            };
        }
    }
}
