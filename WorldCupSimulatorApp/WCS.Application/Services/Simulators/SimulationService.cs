using WCS.Application.DTO.MatchesDTO;
using WCS.Application.DTO.ProbabilitiesDTO;
using WCS.Application.DTO.RatingsDTO;
using WCS.Application.Services.Probabilities;
using WCS.Application.Services.Ratings;
using WCS.Domain.Enums;

namespace WCS.Application.Services.Simulators
{
    public class SimulationService(IMatchProbabilityService matchProbabilityService, IRatingService ratingService) : ISimulationService
    {
        private readonly IMatchProbabilityService _matchProbabilityService = matchProbabilityService;
        private readonly IRatingService _ratingService = ratingService;

        public const int MaxGoals = 6;

        private MatchProbabilityDTO GetProbabilities(SimpleSimulationMatchDTO match)
        {
            var lambdaA = _matchProbabilityService.CalculateLambda(match.AAttackRating, match.BDefenseRating);
            var lambdaB = _matchProbabilityService.CalculateLambda(match.BAttackRating, match.ADefenseRating);
            return _matchProbabilityService.CalculateMatchProbabilities(MaxGoals, lambdaA, lambdaB);
        }

        private static SimpleMatchResultDTO SimpleBuildResult(SimpleSimulationMatchDTO match, MatchOutcome outcome,
            MatchProbabilityDTO matchProbability)
        {
            var result = new SimpleMatchResultDTO
            {
                TeamA = match.TeamA,
                TeamB = match.TeamB
            };

            if (outcome == MatchOutcome.WinA)
            {
                result.Winner = MatchOutcome.WinA;
                result.WinnerID = match.TeamAID;
                result.OutcomeProbability = matchProbability.WinA;
            }
            else if (outcome == MatchOutcome.WinB)
            {
                result.Winner = MatchOutcome.WinB;
                result.WinnerID = match.TeamBID;
                result.OutcomeProbability = matchProbability.WinB;
            }
            else
            {
                result.Winner = MatchOutcome.Draw;
                result.WinnerID = null;
                result.OutcomeProbability = matchProbability.Draw;
            }

            return result;
        }

        private static MatchResultDTO BuildResult(SimpleSimulationMatchDTO match, ScoreProbabilityDTO score,
            MatchProbabilityDTO matchProbability)
        {
            var result = new MatchResultDTO
            {
                TeamA = match.TeamA,
                TeamB = match.TeamB,
                GoalsA = score.GoalsA,
                GoalsB = score.GoalsB,
                ScoreProbability = score.Probability,
            };

            if (score.GoalsA > score.GoalsB)
            {
                result.Winner = MatchOutcome.WinA;
                result.WinnerID = match.TeamAID;
                result.OutcomeProbability = matchProbability.WinA;
            }
            else if (score.GoalsB > score.GoalsA)
            {
                result.Winner = MatchOutcome.WinB;
                result.WinnerID = match.TeamBID;
                result.OutcomeProbability = matchProbability.WinB;
            }
            
            return result;
        }

        public List<SimpleMatchResultDTO> SimpleSimulateGroupsStage (List<SimpleSimulationMatchDTO> matches)
        {
            if (matches == null || matches.Count == 0)
                throw new ArgumentException("Matches list is empty.");

            var results = new List<SimpleMatchResultDTO>();

            foreach (var match in matches)
            {
                var matchProbability = GetProbabilities(match);
                var winner = _matchProbabilityService.PickRandomOutcome(matchProbability);
                var result = SimpleBuildResult(match, winner, matchProbability);

                results.Add(result);
            }

            return results;
        }

        public List<SimpleMatchResultDTO> SimpleSimulateKnockouts(List<SimpleSimulationMatchDTO> matches)
        {
            if (matches == null || matches.Count == 0)
                throw new ArgumentException("Matches list is empty.");

            var results = new List<SimpleMatchResultDTO>();

            foreach (var match in matches)
            {
                var matchProbability = GetProbabilities(match);

                double totalProbability = matchProbability.WinA + matchProbability.WinB;
                double AProbability = matchProbability.WinA / totalProbability;

                double roll = Random.Shared.NextDouble();

                var winner = roll <= AProbability ? MatchOutcome.WinA : MatchOutcome.WinB;

                var result = SimpleBuildResult(match, winner, matchProbability);

                results.Add(result);
            }

            return results;
        }

        public List<MatchResultDTO> SimpleSimulateGroupsStageWithScores(List<SimpleSimulationMatchDTO> matches)
        {
            if (matches == null || matches.Count == 0)
                throw new ArgumentException("Matches list is empty.");

            var results = new List<MatchResultDTO>();

            foreach (var match in matches)
            {
                var matchProbability = GetProbabilities(match);
                var score = _matchProbabilityService.PickRandomScore(matchProbability.Scores);

                var result = BuildResult(match, score, matchProbability);

                if (score.GoalsA == score.GoalsB)
                {
                    result.Winner = MatchOutcome.Draw;
                    result.WinnerID = null;
                    result.OutcomeProbability = matchProbability.Draw;
                }

                results.Add(result);
            }

            return results;
        }

        public List<MatchResultDTO> SimpleSimulateKnockoutsWithScores(List<SimpleSimulationMatchDTO> matches)
        {
            if (matches == null || matches.Count == 0)
                throw new ArgumentException("Matches list is empty.");

            var results = new List<MatchResultDTO>();

            foreach (var match in matches)
            {
                var matchProbability = GetProbabilities(match);
                var score = _matchProbabilityService.PickRandomScore(matchProbability.Scores);

                double totalProbability = matchProbability.WinA + matchProbability.WinB;
                double AProbability = matchProbability.WinA / totalProbability;
                double BProbability = matchProbability.WinB / totalProbability;

                double roll = Random.Shared.NextDouble();
                var winner = roll <= AProbability ? MatchOutcome.WinA : MatchOutcome.WinB;

                var result = BuildResult(match, score, matchProbability);

                if (score.GoalsA == score.GoalsB)                
                {
                    result.Winner = winner;

                    if (winner == MatchOutcome.WinA)
                    {
                        result.WinnerID = match.TeamAID;
                        result.OutcomeProbability = AProbability;
                        result.DecidedByPenalties = true;
                    } else
                    {
                        result.WinnerID = match.TeamBID;
                        result.OutcomeProbability = BProbability;
                        result.DecidedByPenalties = true;
                    }
                }
                results.Add(result);
            }
            return results;
        }

        public List<AdaptativeMatchResultDTO> SimulateAdaptativeKnockoutsWithScores(List<AdaptativeSimulationMatchDTO> matches,
            List<RatingDataDTO> previousResults)
        {
            if (matches == null || matches.Count == 0)
                throw new ArgumentException("Matches list is empty.");

            var results = new List<AdaptativeMatchResultDTO>();            

            foreach (var match in matches)
            {
                var result = new AdaptativeMatchResultDTO
                {
                    TeamA = match.TeamA,
                    TeamB = match.TeamB,                    
                };

                var aAttackRating = _ratingService.CalculateAttack(previousResults, match.AAccumulatedScores,
                    match.AAccumulatedWeights);
                var aDefenseRating = _ratingService.CalculateDefense(previousResults, match.AAccumulatedPenalties,
                    match.AAccumulatedCount);
                var bAttackRating = _ratingService.CalculateAttack(previousResults, match.BAccumulatedScores,
                    match.BAccumulatedWeights);
                var bDefenseRating = _ratingService.CalculateDefense(previousResults, match.BAccumulatedPenalties,
                    match.BAccumulatedCount);

                var lambdaA = _matchProbabilityService.CalculateLambda(aAttackRating.AttackRating, bDefenseRating.DefenseRating);
                var lambdaB = _matchProbabilityService.CalculateLambda(bAttackRating.AttackRating, aDefenseRating.DefenseRating);
                 
                var matchProbability = _matchProbabilityService.CalculateMatchProbabilities(MaxGoals, lambdaA, lambdaB);
                var score = _matchProbabilityService.PickRandomScore(matchProbability.Scores);

                result.GoalsA = score.GoalsA;
                result.GoalsB = score.GoalsB;
                result.ScoreProbability = score.Probability;

                double totalProbability = matchProbability.WinA + matchProbability.WinB;
                double AProbability = matchProbability.WinA / totalProbability;
                double BProbability = matchProbability.WinB / totalProbability;

                double roll = Random.Shared.NextDouble();
                var winner = roll <= AProbability ? MatchOutcome.WinA : MatchOutcome.WinB;


                if (score.GoalsA > score.GoalsB)
                {
                    result.Winner = MatchOutcome.WinA;
                    result.WinnerID = match.TeamAID;
                    result.OutcomeProbability = matchProbability.WinA;
                    result.WinnerAccumulatedScores = aAttackRating.AccumulatedScores;
                    result.WinnerAccumulatedWeights = aAttackRating.AccumulatedWeights;
                    result.WinnerAccumulatedPenalties = aDefenseRating.AccumulatedPenalties;
                    result.WinnerAccumulatedCount = aDefenseRating.AccumulatedCount;
                }
                else if (score.GoalsB > score.GoalsA)
                {
                    result.Winner = MatchOutcome.WinB;
                    result.WinnerID = match.TeamBID;
                    result.OutcomeProbability = matchProbability.WinB;
                    result.WinnerAccumulatedScores = bAttackRating.AccumulatedScores;
                    result.WinnerAccumulatedWeights = bAttackRating.AccumulatedWeights;
                    result.WinnerAccumulatedPenalties = bDefenseRating.AccumulatedPenalties;
                    result.WinnerAccumulatedCount = bDefenseRating.AccumulatedCount;
                }
                else 
                {
                    result.Winner = winner;

                    if (winner == MatchOutcome.WinA)
                    {
                        result.WinnerID = match.TeamAID;
                        result.OutcomeProbability = AProbability;
                        result.DecidedByPenalties = true;
                        result.WinnerAccumulatedScores = aAttackRating.AccumulatedScores;
                        result.WinnerAccumulatedWeights = aAttackRating.AccumulatedWeights;
                        result.WinnerAccumulatedPenalties = aDefenseRating.AccumulatedPenalties;
                        result.WinnerAccumulatedCount = aDefenseRating.AccumulatedCount;
                    }
                    else
                    {
                        result.WinnerID = match.TeamBID;
                        result.OutcomeProbability = BProbability;
                        result.DecidedByPenalties = true;
                        result.WinnerAccumulatedScores = bAttackRating.AccumulatedScores;
                        result.WinnerAccumulatedWeights = bAttackRating.AccumulatedWeights;
                        result.WinnerAccumulatedPenalties = bDefenseRating.AccumulatedPenalties;
                        result.WinnerAccumulatedCount = bDefenseRating.AccumulatedCount;
                    }
                }
                results.Add(result);
            }
            return results;
        }
    }
}
