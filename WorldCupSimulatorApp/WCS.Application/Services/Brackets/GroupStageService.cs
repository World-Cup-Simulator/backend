using WCS.Application.Services.Simulators;
using WCS.Application.DTO.Mappers;
using WCS.Domain.Entities;
using WCS.Domain.Enums;

namespace WCS.Application.Services.Brackets
{
    public class GroupStageService(ISimulationService simulationService)
    {
        private readonly ISimulationService _simulationService = simulationService;

        public static List<GroupTable> BuildGroups(List<WorldCupTeam> teams)
        {
            if (teams.Count == 0)
                throw new ArgumentException("Teams list is empty.");

            return teams
                .GroupBy(t => t.GroupCode)
                .OrderBy(g => g.Key)
                .Select(g => new GroupTable
                {
                    GroupCode = g.Key,
                    Teams = g.Select(t => new GroupTableEntry
                    {
                        TeamId = t.TeamId,
                        Name = t.Team.Name
                    }).ToList()
                })
                .ToList();
        }

        public void SimpleUpdateGroups(List<WorldCupMatch> matches, List<GroupTable> groupsTable)
        {
            var simulationList = BracketsMappers.CreateGroupMatches(matches);

            var results = _simulationService.SimpleSimulateGroupsStage(simulationList);

            var teamLookup = groupsTable
                .SelectMany(g => g.Teams)
                .ToDictionary(t => t.TeamId);

            foreach (var result in results)
            {
                var teamA = teamLookup[result.TeamAID];
                var teamB = teamLookup[result.TeamBID];

                if (result.Winner == MatchOutcome.WinA)
                {
                    teamA.Points += 3;
                }
                else if (result.Winner == MatchOutcome.WinB)
                {
                    teamB.Points += 3;
                }
                else // Draw
                {
                    teamA.Points += 1;
                    teamB.Points += 1;
                }
            }
        }

        public void UpdateGroupsWithScores(List<WorldCupMatch> matches, List<GroupTable> groupsTable)
        {
            var simulationList = BracketsMappers.CreateGroupMatches(matches);

            var results = _simulationService.SimpleSimulateGroupsStageWithScores(simulationList);

            var teamLookup = groupsTable
                .SelectMany(g => g.Teams)
                .ToDictionary(t => t.TeamId);

            foreach (var result in results)
            {
                var teamA = teamLookup[result.TeamAID];
                var teamB = teamLookup[result.TeamBID];

                if (result.Winner == MatchOutcome.WinA)
                {
                    teamA.Points += 3;
                    BracketsMappers.AssignGoals(teamA, teamB, result);
                }
                else if (result.Winner == MatchOutcome.WinB)
                {
                    teamB.Points += 3;
                    BracketsMappers.AssignGoals(teamA, teamB, result);
                }
                else // Draw
                {
                    teamA.Points += 1;
                    teamB.Points += 1;
                    BracketsMappers.AssignGoals(teamA, teamB, result);
                }
            }
        }
    }
}
