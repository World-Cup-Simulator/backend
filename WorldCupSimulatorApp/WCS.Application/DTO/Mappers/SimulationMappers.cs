using WCS.Application.DTO.MatchesDTO;
using WCS.Application.DTO.ProbabilitiesDTO;
using WCS.Application.DTO.RatingsDTO;
using WCS.Domain.Enums;

namespace WCS.Application.DTO.Mappers
{
    public class SimulationMappers
    {
        public static void AssignWinnerAccumulatedData(AdaptativeMatchResultDTO result, MatchOutcome winner, int teamAID,
            int teamBID, double probability, bool penalties, AttackRatingDTO attack, DefenseRatingDTO defense)
        {
            // Map winner outcome + carry over accumulated stats from the winning team
            result.Winner = winner;
            result.TeamAID = teamAID;
            result.TeamBID = teamBID;
            result.OutcomeProbability = probability;
            result.DecidedByPenalties = penalties;
            result.WinnerAccumulatedScores = attack.AccumulatedScores;
            result.WinnerAccumulatedWeights = attack.AccumulatedWeights;
            result.WinnerAccumulatedPenalties = defense.AccumulatedPenalties;
            result.WinnerAccumulatedCount = defense.AccumulatedCount;
        }

        public static SimpleMatchResultDTO SimpleBuildResult(SimpleSimulationMatchDTO match, MatchOutcome outcome,
            MatchProbabilityDTO matchProbability)
        {
            var result = new SimpleMatchResultDTO
            {
                TeamA = match.TeamA,
                TeamB = match.TeamB,
                TeamAID = match.TeamAID,
                TeamBID = match.TeamBID
            };

            if (outcome == MatchOutcome.WinA)
            {
                result.Winner = MatchOutcome.WinA;
                result.OutcomeProbability = matchProbability.WinA;
            }
            else if (outcome == MatchOutcome.WinB)
            {
                result.Winner = MatchOutcome.WinB;
                result.OutcomeProbability = matchProbability.WinB;
            }
            else
            {
                result.Winner = MatchOutcome.Draw;
                result.OutcomeProbability = matchProbability.Draw;
            }

            return result;
        }

        public static MatchResultDTO BuildResult(SimpleSimulationMatchDTO match, ScoreProbabilityDTO score,
            MatchProbabilityDTO matchProbability)
        {
            var result = new MatchResultDTO
            {
                TeamA = match.TeamA,
                TeamB = match.TeamB,
                TeamAID = match.TeamAID,
                TeamBID = match.TeamBID,
                GoalsA = score.GoalsA,
                GoalsB = score.GoalsB,
                ScoreProbability = score.Probability,
            };

            if (score.GoalsA > score.GoalsB)
            {
                result.Winner = MatchOutcome.WinA;
                result.OutcomeProbability = matchProbability.WinA;
            }
            else if (score.GoalsB > score.GoalsA)
            {
                result.Winner = MatchOutcome.WinB;
                result.OutcomeProbability = matchProbability.WinB;
            }

            return result;
        }
    }
}
