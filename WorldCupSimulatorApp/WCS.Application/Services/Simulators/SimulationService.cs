using WCS.Application.DTO.Mappers;
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

        public List<SimpleMatchResultDTO> SimpleSimulateGroupsStage(List<SimpleSimulationMatchDTO> matches)
        {
            ValidateMatches(matches);

            var results = new List<SimpleMatchResultDTO>();

            foreach (var match in matches)
            {
                // Compute probabilities once (no recalculation in simple mode)
                var matchProbability = GetProbabilities(match);
                var winner = _matchProbabilityService.PickRandomOutcome(matchProbability);
                var result = SimulationMappers.SimpleBuildResult(match, winner, matchProbability);

                results.Add(result);
            }

            return results;
        }

        public List<SimpleMatchResultDTO> SimpleSimulateKnockouts(List<SimpleSimulationMatchDTO> matches)
        {
            ValidateMatches(matches);

            var results = new List<SimpleMatchResultDTO>();

            foreach (var match in matches)
            {
                var matchProbability = GetProbabilities(match);
                var winner = _matchProbabilityService.PickRandomOutcome(matchProbability);

                var result = SimulationMappers.SimpleBuildResult(match, winner, matchProbability);

                // Knockout stages cannot end in a draw
                if (winner == MatchOutcome.Draw)
                {
                    var decider = PickKnockoutWinner(matchProbability.WinA, matchProbability.WinB);
                    matchProbability.WinA = decider.AProbability;
                    matchProbability.WinB = decider.BProbability;
                    matchProbability.Draw = 0;
                    result = SimulationMappers.SimpleBuildResult(match, decider.MatchOutcome, matchProbability);
                }

                results.Add(result);
            }

            return results;
        }

        public List<MatchResultDTO> SimpleSimulateGroupsStageWithScores(List<SimpleSimulationMatchDTO> matches)
        {
            ValidateMatches(matches);

            var results = new List<MatchResultDTO>();

            foreach (var match in matches)
            {
                // Compute probabilities once (no recalculation in simple mode)
                var matchProbability = GetProbabilities(match);

                // Randomly select a score based on Poisson distribution
                var score = _matchProbabilityService.PickRandomScore(matchProbability.Scores);

                var result = SimulationMappers.BuildResult(match, score, matchProbability);

                // If score is tied, explicitly mark as draw
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
            ValidateMatches(matches);

            var results = new List<MatchResultDTO>();

            foreach (var match in matches)
            {
                var matchProbability = GetProbabilities(match);
                var score = _matchProbabilityService.PickRandomScore(matchProbability.Scores);                               

                var result = SimulationMappers.BuildResult(match, score, matchProbability);

                // Knockout stages cannot end in a draw
                if (score.GoalsA == score.GoalsB)
                {
                    // Decide winner via penalties
                    var winner = PickKnockoutWinner(matchProbability.WinA, matchProbability.WinB);

                    result.Winner = winner.MatchOutcome;

                    if (winner.MatchOutcome == MatchOutcome.WinA)
                    {
                        result.WinnerID = match.TeamAID;
                        // In knockout draws, probability is normalized between A and B (penalty shootout scenario)
                        result.OutcomeProbability = winner.AProbability;
                        result.DecidedByPenalties = true;
                    }
                    else
                    {
                        result.WinnerID = match.TeamBID;
                        // In knockout draws, probability is normalized between A and B (penalty shootout scenario)
                        result.OutcomeProbability = winner.BProbability;
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
            ValidateMatches(matches);

            var results = new List<AdaptativeMatchResultDTO>();

            foreach (var match in matches)
            {
                var result = new AdaptativeMatchResultDTO
                {
                    TeamA = match.TeamA,
                    TeamB = match.TeamB,
                };

                // Extract previous data for each team
                var aRatingData = previousResults.Where(r => r.TeamID == match.TeamAID).ToList();
                var bRatingData = previousResults.Where(r => r.TeamID == match.TeamBID).ToList();

                // Compute adaptive ratings and expected goals (lambda) based on past performance
                var AdptRatings = CalculateAdaptativeRatings(match, aRatingData, bRatingData);

                // Generate match probabilities using updated lambdas (Poisson model)
                var matchProbability = _matchProbabilityService.CalculateMatchProbabilities(MaxGoals, AdptRatings.LambdaA,
                    AdptRatings.LambdaB);

                // Sample a score from the probability distribution
                var score = _matchProbabilityService.PickRandomScore(matchProbability.Scores);

                result.GoalsA = score.GoalsA;
                result.GoalsB = score.GoalsB;
                result.ScoreProbability = score.Probability;                              

                if (score.GoalsA > score.GoalsB)
                {
                    // Direct win, use original model probability
                    SimulationMappers.AssignWinnerAccumulatedData(result, MatchOutcome.WinA, match.TeamAID, matchProbability.WinA, false,
                        AdptRatings.AAttackRating, AdptRatings.ADefenseRating);
                }
                else if (score.GoalsB > score.GoalsA)
                {
                    // Direct win, use original model probability
                    SimulationMappers.AssignWinnerAccumulatedData(result, MatchOutcome.WinB, match.TeamBID, matchProbability.WinB, false,
                        AdptRatings.BAttackRating, AdptRatings.BDefenseRating);
                }
                else
                {
                    // Decide winner via penalties
                    var winner = PickKnockoutWinner(matchProbability.WinA, matchProbability.WinB);

                    if (winner.MatchOutcome == MatchOutcome.WinA)
                    {
                        // In knockout draws, probability is normalized between A and B (penalty shootout scenario)
                        SimulationMappers.AssignWinnerAccumulatedData(result, MatchOutcome.WinA, match.TeamAID, winner.AProbability, true,
                            AdptRatings.AAttackRating, AdptRatings.ADefenseRating);
                    }
                    else
                    {
                        // In knockout draws, probability is normalized between A and B (penalty shootout scenario)
                        SimulationMappers.AssignWinnerAccumulatedData(result, MatchOutcome.WinB, match.TeamBID, winner.BProbability, true,
                            AdptRatings.BAttackRating, AdptRatings.BDefenseRating);
                    }
                }
                results.Add(result);
            }
            return results;
        }

        private MatchProbabilityDTO GetProbabilities(SimpleSimulationMatchDTO match)
        {
            // Compute expected goals (lambda) for both teams based on attack vs defense ratings
            var lambdaA = _matchProbabilityService.CalculateLambda(match.AAttackRating, match.BDefenseRating);
            var lambdaB = _matchProbabilityService.CalculateLambda(match.BAttackRating, match.ADefenseRating);

            // Use Poisson model to derive match outcome probabilities up to MaxGoals
            return _matchProbabilityService.CalculateMatchProbabilities(MaxGoals, lambdaA, lambdaB);
        }

        private static void ValidateMatches<T>(List<T> matches)
        {
            if (matches == null || matches.Count == 0)
                throw new ArgumentException("Matches list is empty.");
        }

        private static KnockoutWinnerDTO PickKnockoutWinner(double winA, double winB)
        {
            if (winA == 0 && winB == 0)
                throw new ArgumentException("Probabilities cannot be 0.");

            // Normalize probabilities to ensure they sum to 1
            double totalProbability = winA + winB;
            double AProbability = winA / totalProbability;
            double BProbability = winB / totalProbability;

            // Random draw to decide winner based on normalized probabilities
            double roll = Random.Shared.NextDouble();

            var winner = roll <= AProbability ? MatchOutcome.WinA : MatchOutcome.WinB;

            return new KnockoutWinnerDTO
            {
                MatchOutcome = winner,
                AProbability = AProbability,
                BProbability = BProbability,
            };
        }

        private AdaptativeRatingsDTO CalculateAdaptativeRatings(AdaptativeSimulationMatchDTO match,
                List<RatingDataDTO> aRatingData, List<RatingDataDTO> bRatingData)
        {
            // Compute dynamic (adaptive) attack/defense ratings based on historical data + accumulated stats
            var aAttackRating = _ratingService.CalculateAttack(aRatingData, match.AAccumulatedScores,
                    match.AAccumulatedWeights);
            var aDefenseRating = _ratingService.CalculateDefense(aRatingData, match.AAccumulatedPenalties,
                match.AAccumulatedCount);
            var bAttackRating = _ratingService.CalculateAttack(bRatingData, match.BAccumulatedScores,
                match.BAccumulatedWeights);
            var bDefenseRating = _ratingService.CalculateDefense(bRatingData, match.BAccumulatedPenalties,
                match.BAccumulatedCount);

            // Convert ratings into expected goals (Poisson lambda)
            var lambdaA = _matchProbabilityService.CalculateLambda(aAttackRating.AttackRating, bDefenseRating.DefenseRating);
            var lambdaB = _matchProbabilityService.CalculateLambda(bAttackRating.AttackRating, aDefenseRating.DefenseRating);

            // Return both lambdas and full rating breakdown for later use (e.g., calculate match probabilities)
            return new AdaptativeRatingsDTO
            {
                LambdaA = lambdaA,
                LambdaB = lambdaB,
                AAttackRating = aAttackRating,
                ADefenseRating = aDefenseRating,
                BAttackRating = bAttackRating,
                BDefenseRating = bDefenseRating
            };
        }        
    }
}
