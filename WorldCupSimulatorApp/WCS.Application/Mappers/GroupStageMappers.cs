using WCS.Application.DTO.BracketsDTO;
using WCS.Application.DTO.SimulatorsDTO;
using WCS.Domain.Entities;

namespace WCS.Application.Mappers
{
    public static class GroupStageMappers
    {        
        public static GroupResultDTO CreateGroupResult(IMatchResult result)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));
            var groupResult = new GroupResultDTO
            {
                TeamA = result.TeamA,
                TeamB = result.TeamB,
                Winner = result.Winner,
                Date = result.Date,
                TeamAID = result.TeamAID,
                TeamBID = result.TeamBID,
                OutcomeProbability = result.OutcomeProbability
            };
            if (result is IScoreResult scoreResult)
            {
                groupResult.GoalsA = scoreResult.GoalsA;
                groupResult.GoalsB = scoreResult.GoalsB;
                groupResult.ScoreProbability = scoreResult.ScoreProbability;
                groupResult.DecidedByPenalties = scoreResult.DecidedByPenalties;
            }
            return groupResult;
        }

        public static void AssignGoals(GroupTableEntry teamA, GroupTableEntry teamB, IMatchResult result)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));
            if (result is not IScoreResult scoreResult)
                return;
            teamA.GoalsScored += scoreResult.GoalsA;
            teamA.GoalsConceded += scoreResult.GoalsB;
            teamB.GoalsScored += scoreResult.GoalsB;
            teamB.GoalsConceded += scoreResult.GoalsA;
        }
    }
}
