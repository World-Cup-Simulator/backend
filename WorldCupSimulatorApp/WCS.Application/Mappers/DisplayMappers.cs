using WCS.Application.DTO.BracketsDTO;
using WCS.Application.DTO.DisplaysDTO;
using WCS.Application.DTO.SimulatorsDTO;
using WCS.Domain.Entities;

namespace WCS.Application.Mappers
{
    public class DisplayMappers
    {
        public static GroupResultDisplayDTO MapGroupResult(GroupResultDTO result)
        {
            return new GroupResultDisplayDTO
            {
                GroupCode = result.GroupCode,
                TeamA = result.TeamA,
                TeamB = result.TeamB,
                GoalsA = result.GoalsA,
                GoalsB = result.GoalsB,
                Winner = result.Winner,
                Date = result.Date,
                OutcomeProbability = result.OutcomeProbability,
                ScoreProbability = result.ScoreProbability,
                DecidedByPenalties = result.DecidedByPenalties
            };
        }

        public static GroupTableDisplayDTO MapGroupTable(GroupTable group)
        {
            return new GroupTableDisplayDTO
            {
                GroupCode = group.GroupCode,
                Teams = group.Teams.Select(t => new GroupTableTeamDisplayDTO
                {
                    Name = t.Name,
                    Points = t.Points,
                    GoalsScored = t.GoalsScored,
                    GoalsConceded = t.GoalsConceded
                }).ToList()
            };
        }

        public static KnockoutResultDisplayDTO MapKnockoutResult(IMatchResult result)
        {
            var display = new KnockoutResultDisplayDTO
            {
                TeamA = result.TeamA,
                TeamB = result.TeamB,
                Winner = result.Winner,
                OutcomeProbability = result.OutcomeProbability
            };

            if (result is IScoreResult scoreResult)
            {
                display.GoalsA = scoreResult.GoalsA;
                display.GoalsB = scoreResult.GoalsB;
                display.ScoreProbability = scoreResult.ScoreProbability;
                display.DecidedByPenalties = scoreResult.DecidedByPenalties;
            }

            return display;
        }
    }
}
