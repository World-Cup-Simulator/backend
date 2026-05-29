using WCS.Application.DTO.BracketsDTO;
using WCS.Application.DTO.MatchesDTO;
using WCS.Application.DTO.RatingsDTO;
using WCS.Application.Services.Ratings;
using WCS.Domain.Enums;

namespace WCS.Application.Mappers
{
    public static class KnockoutsMappers
    {
        public static List<SimulationMatchDTO> CreateKnockoutsSimulationMatches(List<KnockoutMatchDTO> matches)
        {
            if (matches is null)
                throw new ArgumentNullException(nameof(matches));

            return matches.Select(CreateKnockoutSimulationMatch).ToList();
        }

        public static List<KnockoutMatchDTO> CreateNextKnockoutsMatches(List<KnockoutMatchDTO> matches, List<IMatchResult> results)
        {
            if (matches is null)
                throw new ArgumentNullException(nameof(matches));

            if (results is null)
                throw new ArgumentNullException(nameof(results));

            if (matches.Count == 0)
                throw new ArgumentException("Matches list cannot be empty.", nameof(matches));

            // After Final (Stage 5), no next round
            var currentStage = matches.First().Stage;

            if (currentStage >= (int)Stage.Final)
                return [];

            var nextMatches = new List<KnockoutMatchDTO>();
            var lastBracket = Math.Max(matches.Last().NextMatchKey, 1);
            var actualKey = matches.First().Key;

            for (int i = 0; i < matches.Count; i += 2)
            {
                var nextMatch = CreateNextMatchFromPair(matches, results, i);
                nextMatches.Add(nextMatch);
            }
            return nextMatches;
        }

        public static List<RatingDataDTO> CreatePreviousKnockoutsResults(List<KnockoutMatchDTO> matches, List<AdaptativeMatchResultDTO> results)
        {
            if (matches is null)
                throw new ArgumentNullException(nameof(matches));

            if (results is null)
                throw new ArgumentNullException(nameof(results));

            return results.Select(r => CreateRatingDataFromKnockoutResult(matches, r)).ToList();
        }

        public static List<RatingDataDTO> CreatePreviousResults(List<GroupResultDTO> results)
        {
            if (results is null)
                throw new ArgumentNullException(nameof(results));

            return results.SelectMany(CreateRatingDataFromGroupResult).ToList();
        }

        private static SimulationMatchDTO CreateKnockoutSimulationMatch(KnockoutMatchDTO match)
        {
            if (match.TeamBID is null)
                throw new InvalidOperationException($"Match {match.Key} is missing Team B assignment.");

            return new SimulationMatchDTO
            {
                TeamAID = match.TeamAID,
                TeamA = match.TeamA,
                AAccumulatedScores = match.AAccumulatedScores,
                AAccumulatedWeights = match.AAccumulatedWeights,
                AAccumulatedPenalties = match.AAccumulatedPenalties,
                AAccumulatedCount = match.AAccumulatedCount,
                TeamBID = match.TeamBID.Value,
                TeamB = match.TeamB,
                BAccumulatedScores = match.BAccumulatedScores,
                BAccumulatedWeights = match.BAccumulatedWeights,
                BAccumulatedPenalties = match.BAccumulatedPenalties,
                BAccumulatedCount = match.BAccumulatedCount
            };
        }

        private static KnockoutMatchDTO CreateNextMatchFromPair(List<KnockoutMatchDTO> matches, List<IMatchResult> results,
            int matchIndex)
        {
            // Ensure we have both matches in the pair
            if (matchIndex + 1 >= matches.Count)
                throw new InvalidOperationException($"Incomplete match pair at index {matchIndex}. Each match must have a paired opponent.");

            var aMatch = matches[matchIndex];
            var bMatch = matches[matchIndex + 1];

            var aResult = results.First(r => r.TeamAID == aMatch.TeamAID);
            var bResult = results.First(r => r.TeamAID == bMatch.TeamAID);

            var nextMatch = new KnockoutMatchDTO
            {
                Key = (matchIndex / 2) + 1,  // Match keys are 1-based in next round
                Stage = aMatch.Stage + 1,
                NextMatchKey = ((matchIndex / 2) + 2) / 2  // Which next-round match this feeds into
            };

            PopulateWinnerProperties(nextMatch, aMatch, aResult, isTeamA: true);
            PopulateWinnerProperties(nextMatch, bMatch, bResult, isTeamA: false);
            return nextMatch;
        }

        private static void PopulateWinnerProperties(KnockoutMatchDTO nextMatch, KnockoutMatchDTO sourceMatch,
            IMatchResult result, bool isTeamA)
        {
            bool winnerIsTeamA = result.Winner == MatchOutcome.WinA;

            if (isTeamA)
            {
                nextMatch.TeamAID = winnerIsTeamA ? sourceMatch.TeamAID : sourceMatch.TeamBID!.Value;
                nextMatch.TeamA = winnerIsTeamA ? sourceMatch.TeamA : sourceMatch.TeamB;
                nextMatch.TeamAFifaRank = winnerIsTeamA ? sourceMatch.TeamAFifaRank : sourceMatch.TeamBFifaRank;
                nextMatch.AAccumulatedScores = winnerIsTeamA ? sourceMatch.AAccumulatedScores : sourceMatch.BAccumulatedScores;
                nextMatch.AAccumulatedWeights = winnerIsTeamA ? sourceMatch.AAccumulatedWeights : sourceMatch.BAccumulatedWeights;
                nextMatch.AAccumulatedPenalties = winnerIsTeamA ? sourceMatch.AAccumulatedPenalties : sourceMatch.BAccumulatedPenalties;
                nextMatch.AAccumulatedCount = winnerIsTeamA ? sourceMatch.AAccumulatedCount : sourceMatch.BAccumulatedCount;
            }
            else
            {
                nextMatch.TeamBID = winnerIsTeamA ? sourceMatch.TeamAID : sourceMatch.TeamBID!.Value;
                nextMatch.TeamB = winnerIsTeamA ? sourceMatch.TeamA : sourceMatch.TeamB;
                nextMatch.TeamBFifaRank = winnerIsTeamA ? sourceMatch.TeamAFifaRank : sourceMatch.TeamBFifaRank;
                nextMatch.BAccumulatedScores = winnerIsTeamA ? sourceMatch.AAccumulatedScores : sourceMatch.BAccumulatedScores;
                nextMatch.BAccumulatedWeights = winnerIsTeamA ? sourceMatch.AAccumulatedWeights : sourceMatch.BAccumulatedWeights;
                nextMatch.BAccumulatedPenalties = winnerIsTeamA ? sourceMatch.AAccumulatedPenalties : sourceMatch.BAccumulatedPenalties;
                nextMatch.BAccumulatedCount = winnerIsTeamA ? sourceMatch.AAccumulatedCount : sourceMatch.BAccumulatedCount;
            }
        }

        private static IEnumerable<RatingDataDTO> CreateRatingDataFromGroupResult(GroupResultDTO result)
        {
            yield return new RatingDataDTO
            {
                TeamID = result.TeamAID,
                GoalsScored = result.GoalsA,
                GoalsConceded = result.GoalsB,
                OpponentFifaRank = result.TeamBFifaRank,
                OpponentAttackRating = result.BAttackRating,
                Date = result.Date,
                Competition = Competition.WorldCup,
                Stage = Stage.GroupStage
            };

            yield return new RatingDataDTO
            {
                TeamID = result.TeamBID,
                GoalsScored = result.GoalsB,
                GoalsConceded = result.GoalsA,
                OpponentFifaRank = result.TeamAFifaRank,
                OpponentAttackRating = result.AAttackRating,
                Date = result.Date,
                Competition = Competition.WorldCup,
                Stage = Stage.GroupStage
            };
        }

        private static RatingDataDTO CreateRatingDataFromKnockoutResult(List<KnockoutMatchDTO> matches, AdaptativeMatchResultDTO result)
        {
            var match = matches.First(m => m.TeamAID == result.TeamAID);
            bool winnerIsTeamA = result.Winner == MatchOutcome.WinA;
            var winnerTeamId = winnerIsTeamA ? result.TeamAID : result.TeamBID;
            var goalsScored = winnerIsTeamA ? result.GoalsA : result.GoalsB;
            var goalsConceded = winnerIsTeamA ? result.GoalsB : result.GoalsA;
            var opponentRank = winnerIsTeamA ? match.TeamBFifaRank : match.TeamAFifaRank;
            var opponentScores = winnerIsTeamA ? match.BAccumulatedScores : match.AAccumulatedScores;
            var opponentWeights = winnerIsTeamA ? match.BAccumulatedWeights : match.AAccumulatedWeights;

            return new RatingDataDTO
            {
                TeamID = winnerTeamId,
                GoalsScored = goalsScored,
                GoalsConceded = goalsConceded,
                OpponentFifaRank = opponentRank,
                OpponentAttackRating = RatingHelper.CalculateAttackRating(opponentScores, opponentWeights),
                Date = DateOnly.FromDateTime(DateTime.Now),
                Competition = Competition.WorldCup,
                Stage = (Stage)match.Stage
            };
        }
    }
}