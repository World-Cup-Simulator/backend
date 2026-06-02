using WCS.Application.DTO.SimulatorsDTO;
using WCS.Application.DTO.ProbabilitiesDTO;
using WCS.Application.DTO.RatingsDTO;
using WCS.Application.Mappers;
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

        public List<IMatchResult> SimpleSimulateGroupsStage(List<SimulationMatchDTO> matches)
        {
            ValidateMatches(matches);

            return ProcessMatches(matches, match =>
            {
                var probabilities = GetMatchProbabilities(match);
                var outcome = _matchProbabilityService.PickRandomOutcome(probabilities);
                return SimulationMappers.SimpleBuildResult(match, outcome, probabilities);
            });
        }

        public List<IMatchResult> SimpleSimulateKnockouts(List<SimulationMatchDTO> matches)
        {
            ValidateMatches(matches);

            return ProcessMatches(matches, match =>
            {
                var probabilities = GetMatchProbabilities(match);
                var outcome = _matchProbabilityService.PickRandomOutcome(probabilities);

                // Early return: knockout stages cannot end in a draw
                if (outcome == MatchOutcome.Draw)
                {
                    return ResolveKnockoutDrawFromOutcome(match, probabilities);
                }

                return SimulationMappers.SimpleBuildResult(match, outcome, probabilities);
            });
        }

        public List<IMatchResult> SimpleSimulateGroupsStageWithScores(List<SimulationMatchDTO> matches)
        {
            ValidateMatches(matches);

            return ProcessMatches(matches, match =>
            {
                var probabilities = GetMatchProbabilities(match);
                var matchScore = _matchProbabilityService.PickRandomScore(probabilities.Scores);

                return SimulationMappers.BuildResult(match, matchScore, probabilities);
            });
        }

        public List<IMatchResult> SimpleSimulateKnockoutsWithScores(List<SimulationMatchDTO> matches)
        {
            ValidateMatches(matches);

            return ProcessMatches(matches, match =>
            {
                var probabilities = GetMatchProbabilities(match);
                var matchScore = _matchProbabilityService.PickRandomScore(probabilities.Scores);

                // Early return: handle knockout draws via penalties
                if (matchScore.GoalsA == matchScore.GoalsB)
                {
                    return ResolveKnockoutDrawFromScore(match, matchScore, probabilities);
                }

                return SimulationMappers.BuildResult(match, matchScore, probabilities);
            });
        }

        public List<IMatchResult> SimulateAdaptativeKnockoutsWithScores(List<SimulationMatchDTO> matches, List<RatingDataDTO> previousResults)
        {
            ValidateMatches(matches);
            ValidatePreviousResults(previousResults);

            return ProcessMatches(matches, match =>
            {
                var adaptiveData = CalculateAdaptiveMatchData(match, previousResults);
                var probabilities = _matchProbabilityService.CalculateMatchProbabilities(
                    MaxGoals, adaptiveData.LambdaA, adaptiveData.LambdaB);
                var matchScore = _matchProbabilityService.PickRandomScore(probabilities.Scores);

                return BuildAdaptiveResult(match, matchScore, probabilities, adaptiveData);
            });
        }

        // Generic match processing pipeline that applies a processor function to each match.
        private static List<IMatchResult> ProcessMatches<TInput, TResult>(List<TInput> matches, Func<TInput, TResult> processor)
            where TResult : IMatchResult
        {
            var results = new List<IMatchResult>(matches.Count);

            foreach (var match in matches)
            {
                results.Add(processor(match));
            }

            return results;
        }

        // Calculates match probabilities based on team ratings.
        private MatchProbabilityDTO GetMatchProbabilities(SimulationMatchDTO match)
        {
            // Compute attack/defense ratings based on accumulated stats
            var teamAAttack = _ratingService.CalculateAttack(
                [], match.AAccumulatedScores, match.AAccumulatedWeights);
            var teamADefense = _ratingService.CalculateDefense(
                [], match.AAccumulatedPenalties, match.AAccumulatedCount);
            var teamBAttack = _ratingService.CalculateAttack(
                [], match.BAccumulatedScores, match.BAccumulatedWeights);
            var teamBDefense = _ratingService.CalculateDefense(
                [], match.BAccumulatedPenalties, match.BAccumulatedCount);

            // Convert ratings into expected goals (Poisson lambda)
            var lambdaA = _matchProbabilityService.CalculateLambda(
                teamAAttack.AttackRating, teamBDefense.DefenseRating);
            var lambdaB = _matchProbabilityService.CalculateLambda(
                teamBAttack.AttackRating, teamADefense.DefenseRating);

            // Use Poisson model to derive match outcome probabilities
            return _matchProbabilityService.CalculateMatchProbabilities(MaxGoals, lambdaA, lambdaB);
        }

        // Calculates adaptive ratings and expected goals based on historical performance data.
        private AdaptiveMatchData CalculateAdaptiveMatchData(SimulationMatchDTO match, List<RatingDataDTO> previousResults)
        {
            // Extract previous data for each team
            var teamAHistory = previousResults
                .Where(r => r.TeamID == match.TeamAID)
                .ToList();
            var teamBHistory = previousResults
                .Where(r => r.TeamID == match.TeamBID)
                .ToList();

            // Compute dynamic (adaptive) attack/defense ratings
            var teamAAttack = _ratingService.CalculateAttack(
                teamAHistory, match.AAccumulatedScores, match.AAccumulatedWeights);
            var teamADefense = _ratingService.CalculateDefense(
                teamAHistory, match.AAccumulatedPenalties, match.AAccumulatedCount);
            var teamBAttack = _ratingService.CalculateAttack(
                teamBHistory, match.BAccumulatedScores, match.BAccumulatedWeights);
            var teamBDefense = _ratingService.CalculateDefense(
                teamBHistory, match.BAccumulatedPenalties, match.BAccumulatedCount);

            // Convert ratings into expected goals (Poisson lambda)
            var lambdaA = _matchProbabilityService.CalculateLambda(
                teamAAttack.AttackRating, teamBDefense.DefenseRating);
            var lambdaB = _matchProbabilityService.CalculateLambda(
                teamBAttack.AttackRating, teamADefense.DefenseRating);

            return new AdaptiveMatchData(
                lambdaA, lambdaB,
                teamAAttack, teamADefense,
                teamBAttack, teamBDefense);
        }

        // Resolves a knockout draw when the initial outcome is a Draw.
        // Normalizes probabilities between Team A and Team B win.
        private static SimpleMatchResultDTO ResolveKnockoutDrawFromOutcome(SimulationMatchDTO match, MatchProbabilityDTO probabilities)
        {
            var normalizedWinner = ResolveKnockoutWinner(probabilities.WinA, probabilities.WinB);

            var resolvedProbabilities = new MatchProbabilityDTO
            {
                WinA = normalizedWinner.AProbability,
                WinB = normalizedWinner.BProbability,
                Draw = 0,
                Scores = probabilities.Scores
            };

            return SimulationMappers.SimpleBuildResult(match, normalizedWinner.MatchOutcome, resolvedProbabilities);
        }

        // Resolves a knockout draw when the score is tied.
        // Determines winner via penalty shootout simulation.
        private static MatchResultDTO ResolveKnockoutDrawFromScore(SimulationMatchDTO match, ScoreProbabilityDTO matchScore,
            MatchProbabilityDTO probabilities)
        {
            var penaltyWinner = ResolveKnockoutWinner(probabilities.WinA, probabilities.WinB);

            var matchResult = SimulationMappers.BuildResult(match, matchScore, probabilities);

            matchResult.Winner = penaltyWinner.MatchOutcome;

            matchResult.OutcomeProbability = penaltyWinner.MatchOutcome == MatchOutcome.WinA
                ? penaltyWinner.AProbability
                : penaltyWinner.BProbability;

            matchResult.DecidedByPenalties = true;

            return matchResult;
        }

        // Determines a knockout winner by normalizing win probabilities.
        private static KnockoutWinnerDTO ResolveKnockoutWinner(double winA, double winB)
        {
            if (winA == 0 && winB == 0)
            {
                throw new ArgumentException(
                    "Cannot resolve knockout winner: both WinA and WinB probabilities are zero.", nameof(winA));
            }

            double totalProbability = winA + winB;
            double normalizedA = winA / totalProbability;
            double normalizedB = winB / totalProbability;

            double roll = Random.Shared.NextDouble();
            var winner = roll <= normalizedA ? MatchOutcome.WinA : MatchOutcome.WinB;

            return new KnockoutWinnerDTO
            {
                MatchOutcome = winner,
                AProbability = normalizedA,
                BProbability = normalizedB,
            };
        }

        // Builds an adaptive match result with proper winner assignment.
        private static AdaptativeMatchResultDTO BuildAdaptiveResult(SimulationMatchDTO match, ScoreProbabilityDTO matchScore,
            MatchProbabilityDTO probabilities, AdaptiveMatchData adaptiveData)
        {
            var baseResult = new AdaptativeMatchResultDTO
            {
                TeamA = match.TeamA,
                TeamB = match.TeamB,
                GoalsA = matchScore.GoalsA,
                GoalsB = matchScore.GoalsB,
                ScoreProbability = matchScore.Probability
            };

            // Early return: Team A wins directly
            if (matchScore.GoalsA > matchScore.GoalsB)
            {
                return CreateAdaptativeResultWithBase(
                    baseResult, MatchOutcome.WinA, match.TeamAID, match.TeamBID,
                    probabilities.WinA, false, adaptiveData.TeamAAttack, adaptiveData.TeamADefense);
            }

            // Early return: Team B wins directly
            if (matchScore.GoalsB > matchScore.GoalsA)
            {
                return CreateAdaptativeResultWithBase(
                    baseResult, MatchOutcome.WinB, match.TeamAID, match.TeamBID,
                    probabilities.WinB, false, adaptiveData.TeamBAttack, adaptiveData.TeamBDefense);
            }

            // Draw: resolve via penalties
            var penaltyWinner = ResolveKnockoutWinner(probabilities.WinA, probabilities.WinB);
            var isTeamAWinner = penaltyWinner.MatchOutcome == MatchOutcome.WinA;

            return CreateAdaptativeResultWithBase(
                baseResult,
                penaltyWinner.MatchOutcome,
                match.TeamAID,
                match.TeamBID,
                isTeamAWinner ? penaltyWinner.AProbability : penaltyWinner.BProbability,
                true,
                isTeamAWinner ? adaptiveData.TeamAAttack : adaptiveData.TeamBAttack,
                isTeamAWinner ? adaptiveData.TeamADefense : adaptiveData.TeamBDefense);
        }


        // Helper to merge base result data with winner-specific adaptive data.
        private static AdaptativeMatchResultDTO CreateAdaptativeResultWithBase(AdaptativeMatchResultDTO baseResult, MatchOutcome winner,
            int teamAID, int teamBID, double probability, bool decidedByPenalties, AttackRatingDTO winnerAttack, DefenseRatingDTO winnerDefense)
        {
            var result = SimulationMappers.BuildAdaptativeResult(winner, teamAID, teamBID, probability, decidedByPenalties,
                winnerAttack, winnerDefense);

            result.TeamA = baseResult.TeamA;
            result.TeamB = baseResult.TeamB;
            result.GoalsA = baseResult.GoalsA;
            result.GoalsB = baseResult.GoalsB;
            result.ScoreProbability = baseResult.ScoreProbability;

            return result;
        }

        private static void ValidateMatches<T>(List<T> matches)
        {
            if (matches is null)
            {
                throw new ArgumentNullException(nameof(matches), "Matches list cannot be null.");
            }
            if (matches.Count == 0)
            {
                throw new ArgumentException("Matches list is empty.", nameof(matches));
            }
        }

        private static void ValidatePreviousResults(List<RatingDataDTO> previousResults)
        {
            if (previousResults is null)
            {
                throw new ArgumentNullException(nameof(previousResults), "Previous results cannot be null.");
            }
        }

        // Immutable container for adaptive match calculation data.
        private readonly record struct AdaptiveMatchData(
            double LambdaA,
            double LambdaB,
            AttackRatingDTO TeamAAttack,
            DefenseRatingDTO TeamADefense,
            AttackRatingDTO TeamBAttack,
            DefenseRatingDTO TeamBDefense);
    }
}
