using WCS.Application.DTO.BracketsDTO;
using WCS.Application.DTO.MatchesDTO;
using WCS.Application.DTO.RatingsDTO;
using WCS.Application.Services.Ratings;
using WCS.Domain.Enums;

namespace WCS.Application.Mappers
{
    public class KnockoutsMappers(IRatingService ratingService) : IKnockoutsMappers
    {
        private readonly IRatingService _ratingService = ratingService;
        public static List<SimulationMatchDTO> CreateKnockoutsSimulationMatches(List<KnockoutMatchDTO> matches)
        {
            var simulationList = new List<SimulationMatchDTO>();

            foreach (var match in matches)
            {
                var simulationMatch = new SimulationMatchDTO
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

                simulationList.Add(simulationMatch);
            }

            return simulationList;
        }

        public static List<KnockoutMatchDTO> CreateNextKnockoutsMatches(List<KnockoutMatchDTO> matches, List<IMatchResult> results)
        {
            var nextMatches = new List<KnockoutMatchDTO>();

            var lastBracket = Math.Max(matches.Last().NextMatchKey, 1);
            var ActualKey = matches.First().Key;

            for (int i = 1; i <= lastBracket; i++)
            {
                var nextMatch = new KnockoutMatchDTO();
                var AActualMatch = matches[ActualKey];
                ActualKey += 1;

                nextMatch.Key = i;
                nextMatch.Stage = AActualMatch.Stage + 1;

                var AResult = results
                    .First(r => r.TeamAID == AActualMatch.TeamAID);

                if (AResult.Winner == MatchOutcome.WinA)
                {                    
                    nextMatch.TeamAID = AActualMatch.TeamAID;
                    nextMatch.TeamA = AActualMatch.TeamA;
                    nextMatch.TeamAFifaRank = AActualMatch.TeamAFifaRank;
                    nextMatch.AAccumulatedScores = AActualMatch.AAccumulatedScores;
                    nextMatch.AAccumulatedWeights = AActualMatch.AAccumulatedWeights;
                    nextMatch.AAccumulatedPenalties = AActualMatch.AAccumulatedPenalties;
                    nextMatch.AAccumulatedCount = AActualMatch.AAccumulatedCount;
                } else
                {
                    nextMatch.TeamAID = AActualMatch.TeamBID.Value;
                    nextMatch.TeamA = AActualMatch.TeamB;
                    nextMatch.TeamAFifaRank = AActualMatch.TeamBFifaRank;
                    nextMatch.AAccumulatedScores = AActualMatch.BAccumulatedScores;
                    nextMatch.AAccumulatedWeights = AActualMatch.BAccumulatedWeights;
                    nextMatch.AAccumulatedPenalties = AActualMatch.BAccumulatedPenalties;
                    nextMatch.AAccumulatedCount = AActualMatch.BAccumulatedCount;
                }

                var BActualMatch = matches[ActualKey];
                nextMatch.NextMatchKey = ActualKey/2;
                ActualKey = +1;

                var BResult = results
                    .First(r => r.TeamAID == BActualMatch.TeamAID);

                if (BResult.Winner == MatchOutcome.WinA)
                {
                    nextMatch.TeamBID = BActualMatch.TeamAID;
                    nextMatch.TeamB = BActualMatch.TeamA;
                    nextMatch.TeamBFifaRank = BActualMatch.TeamAFifaRank;
                    nextMatch.BAccumulatedScores = BActualMatch.AAccumulatedScores;
                    nextMatch.BAccumulatedWeights = BActualMatch.AAccumulatedWeights;
                    nextMatch.BAccumulatedPenalties = BActualMatch.AAccumulatedPenalties;
                    nextMatch.BAccumulatedCount = BActualMatch.AAccumulatedCount;
                }
                else
                {
                    nextMatch.TeamBID = BActualMatch.TeamBID.Value;
                    nextMatch.TeamB = BActualMatch.TeamB;
                    nextMatch.TeamBFifaRank = BActualMatch.TeamBFifaRank;
                    nextMatch.BAccumulatedScores = BActualMatch.BAccumulatedScores;
                    nextMatch.BAccumulatedWeights = BActualMatch.BAccumulatedWeights;
                    nextMatch.BAccumulatedPenalties = BActualMatch.BAccumulatedPenalties;
                    nextMatch.BAccumulatedCount = BActualMatch.BAccumulatedCount;
                }

                nextMatches.Add(nextMatch);
            }

            return nextMatches;
        }

        public static List<RatingDataDTO> CreatePreviousResults(List<GroupResultDTO> results)
        {
            var previousResults = new List<RatingDataDTO>();

            foreach (var result in results)
            {
                var previousResultA = new RatingDataDTO
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

                var previousResultB = new RatingDataDTO
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

                previousResults.Add(previousResultA);
                previousResults.Add(previousResultB);
            }

            return previousResults;
        }

        public List<RatingDataDTO> CreatePreviousKnockoutsResults (List<KnockoutMatchDTO> matches, List<AdaptativeMatchResultDTO> results)
        {
            var previousResults = new List<RatingDataDTO>();
            
            foreach (var result in results)
            {
                var previousResult = new RatingDataDTO
                {
                    TeamID = result.TeamAID,
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Competition = Competition.WorldCup,
                    Stage = Stage.GroupStage
                };

                var actualMatch = matches
                    .First(m => m.TeamAID == result.TeamAID);

                if (result.Winner == MatchOutcome.WinA)
                {
                    previousResult.TeamID = result.TeamAID;
                    previousResult.GoalsScored = result.GoalsA;
                    previousResult.GoalsConceded = result.GoalsB;
                    previousResult.OpponentFifaRank = actualMatch.TeamBFifaRank;
                    var bRating = _ratingService.CalculateAttack([], actualMatch.BAccumulatedScores, actualMatch.BAccumulatedWeights);
                    previousResult.OpponentAttackRating = bRating.AttackRating;
                } else
                {
                    previousResult.TeamID = result.TeamBID;
                    previousResult.GoalsScored = result.GoalsB;
                    previousResult.GoalsConceded = result.GoalsA;
                    previousResult.OpponentFifaRank = actualMatch.TeamAFifaRank;
                    var aRating = _ratingService.CalculateAttack([], actualMatch.AAccumulatedScores, actualMatch.AAccumulatedWeights);
                    previousResult.OpponentAttackRating = aRating.AttackRating;
                }

                previousResults.Add(previousResult);
            }

            return previousResults;
        }
    }
}
