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
                var matchProbability = GetProbabilities(match);
                var winner = _matchProbabilityService.PickRandomOutcome(matchProbability);
                var result = SimpleBuildResult(match, winner, matchProbability);

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
                var winner = PickKnockoutWinner(matchProbability.WinA, matchProbability.WinB);
                matchProbability.WinA = winner.AProbability;
                matchProbability.WinB = winner.BProbability;
                matchProbability.Draw = 0;
                var result = SimpleBuildResult(match, winner.MatchOutcome, matchProbability);

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
            ValidateMatches(matches);

            var results = new List<MatchResultDTO>();

            foreach (var match in matches)
            {
                var matchProbability = GetProbabilities(match);
                var score = _matchProbabilityService.PickRandomScore(matchProbability.Scores);
                var winner = PickKnockoutWinner(matchProbability.WinA, matchProbability.WinB);

                var result = BuildResult(match, score, matchProbability);

                if (score.GoalsA == score.GoalsB)
                {
                    result.Winner = winner.MatchOutcome;

                    if (winner.MatchOutcome == MatchOutcome.WinA)
                    {
                        result.WinnerID = match.TeamAID;
                        result.OutcomeProbability = winner.AProbability;
                        result.DecidedByPenalties = true;
                    }
                    else
                    {
                        result.WinnerID = match.TeamBID;
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

                var aRatingData = previousResults.Where(r => r.TeamID == match.TeamAID).ToList();
                var bRatingData = previousResults.Where(r => r.TeamID == match.TeamBID).ToList();

                var AdptRatings = CalculateAdaptativeProbabilities(match, aRatingData, bRatingData);

                var matchProbability = _matchProbabilityService.CalculateMatchProbabilities(MaxGoals, AdptRatings.LambdaA, AdptRatings.LambdaB);
                var score = _matchProbabilityService.PickRandomScore(matchProbability.Scores);

                result.GoalsA = score.GoalsA;
                result.GoalsB = score.GoalsB;
                result.ScoreProbability = score.Probability;

                var winner = PickKnockoutWinner(matchProbability.WinA, matchProbability.WinB);

                if (score.GoalsA > score.GoalsB)
                {
                    AssignWinnerAccumulatedData(result, MatchOutcome.WinA, match.TeamAID, matchProbability.WinA, false,
                        AdptRatings.AAttackRating, AdptRatings.ADefenseRating);
                }
                else if (score.GoalsB > score.GoalsA)
                {
                    AssignWinnerAccumulatedData(result, MatchOutcome.WinB, match.TeamBID, matchProbability.WinB, false,
                        AdptRatings.BAttackRating, AdptRatings.BDefenseRating);
                }
                else
                {
                    if (winner.MatchOutcome == MatchOutcome.WinA)
                    {
                        AssignWinnerAccumulatedData(result, MatchOutcome.WinA, match.TeamAID, winner.AProbability, true,
                            AdptRatings.AAttackRating, AdptRatings.ADefenseRating);
                    }
                    else
                    {
                        AssignWinnerAccumulatedData(result, MatchOutcome.WinB, match.TeamBID, winner.BProbability, true,
                            AdptRatings.BAttackRating, AdptRatings.BDefenseRating);
                    }
                }
                results.Add(result);
            }
            return results;
        }

        private MatchProbabilityDTO GetProbabilities(SimpleSimulationMatchDTO match)
        {
            var lambdaA = _matchProbabilityService.CalculateLambda(match.AAttackRating, match.BDefenseRating);
            var lambdaB = _matchProbabilityService.CalculateLambda(match.BAttackRating, match.ADefenseRating);
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

            double totalProbability = winA + winB;
            double AProbability = winA / totalProbability;
            double BProbability = winB / totalProbability;

            double roll = Random.Shared.NextDouble();

            var winner = roll <= AProbability ? MatchOutcome.WinA : MatchOutcome.WinB;

            return new KnockoutWinnerDTO
            {
                MatchOutcome = winner,
                AProbability = AProbability,
                BProbability = BProbability,
            };
        }

        private AdaptativeRatingsDTO CalculateAdaptativeProbabilities(AdaptativeSimulationMatchDTO match,
                List<RatingDataDTO> aRatingData, List<RatingDataDTO> bRatingData)
        {
            var aAttackRating = _ratingService.CalculateAttack(aRatingData, match.AAccumulatedScores,
                    match.AAccumulatedWeights);
            var aDefenseRating = _ratingService.CalculateDefense(aRatingData, match.AAccumulatedPenalties,
                match.AAccumulatedCount);
            var bAttackRating = _ratingService.CalculateAttack(bRatingData, match.BAccumulatedScores,
                match.BAccumulatedWeights);
            var bDefenseRating = _ratingService.CalculateDefense(bRatingData, match.BAccumulatedPenalties,
                match.BAccumulatedCount);

            var lambdaA = _matchProbabilityService.CalculateLambda(aAttackRating.AttackRating, bDefenseRating.DefenseRating);
            var lambdaB = _matchProbabilityService.CalculateLambda(bAttackRating.AttackRating, aDefenseRating.DefenseRating);

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

        private static void AssignWinnerAccumulatedData(AdaptativeMatchResultDTO result, MatchOutcome winner, int teamID,
            double probability, bool penalties, AttackRatingDTO attack, DefenseRatingDTO defense)
        {
            result.Winner = winner;
            result.WinnerID = teamID;
            result.OutcomeProbability = probability;
            result.DecidedByPenalties = penalties;
            result.WinnerAccumulatedScores = attack.AccumulatedScores;
            result.WinnerAccumulatedWeights = attack.AccumulatedWeights;
            result.WinnerAccumulatedPenalties = defense.AccumulatedPenalties;
            result.WinnerAccumulatedCount = defense.AccumulatedCount;
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
    }
}
