using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WCS.Application.DTO.MatchesDTO;
using WCS.Domain.Entities;

namespace WCS.Application.DTO.Mappers
{
    public class BracketsMappers
    {
        public static List<SimpleSimulationMatchDTO> CreateGroupMatches (List<WorldCupMatch> matches)
        {
            var simulationList = new List<SimpleSimulationMatchDTO>();

            foreach (var match in matches)
            {
                var simulationMatch = new SimpleSimulationMatchDTO
                {
                    TeamAID = match.TeamAId,
                    TeamA = match.TeamA.Team.Name,
                    AAttackRating = match.TeamA.Team.AttackRating,
                    ADefenseRating = match.TeamA.Team.DefenseRating,
                    TeamBID = match.TeamBId,
                    TeamB = match.TeamB.Team.Name,
                    BAttackRating = match.TeamB.Team.AttackRating,
                    BDefenseRating = match.TeamB.Team.DefenseRating
                };

                simulationList.Add(simulationMatch);
            }

            return simulationList;
        }

        public static void AssignGoals (GroupTableEntry teamA, GroupTableEntry teamB, MatchResultDTO result)
        {
            teamA.GoalsScored += result.GoalsA;
            teamA.GoalsConceded += result.GoalsB;
            teamB.GoalsScored += result.GoalsB;
            teamB.GoalsConceded += result.GoalsA;
        }
    }
}
