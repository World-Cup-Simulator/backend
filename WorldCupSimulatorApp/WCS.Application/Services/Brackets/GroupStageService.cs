using WCS.Application.DTO.BracketsDTO;
using WCS.Application.DTO.MatchesDTO;
using WCS.Application.Mappers;
using WCS.Domain.Entities;
using WCS.Domain.Enums;

namespace WCS.Application.Services.Brackets
{
    public class GroupStageService()
    {
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
                        Name = t.Team.Name,
                        AccumulatedScores = t.Team.AccumulatedScores,
                        AccumulatedWeights = t.Team.AccumulatedWeights,
                        AccumulatedPenalties = t.Team.AccumulatedPenalties,
                        AccumulatedCount = t.Team.AccumulatedCount,
                    }).ToList()
                })
                .ToList();
        }

        public static List<GroupResultDTO> UpdateGroups(List<WorldCupMatch> matches, List<GroupTable> groupsTable, Func<List<SimulationMatchDTO>, 
            List<IMatchResult>> simulate)
        {
            var resultsList = new List<GroupResultDTO>();

            var simulationList = BracketsMappers.CreateGroupMatches(matches);

            var results = simulate(simulationList);

            var teamLookup = groupsTable
                .SelectMany(g => g.Teams.Select(t => new
                    {
                        TeamEntry = t,
                    g.GroupCode
                }))
                .ToDictionary(x => x.TeamEntry.TeamId);

            foreach (var result in results)
            {
                var teamA = teamLookup[result.TeamAID];
                var teamB = teamLookup[result.TeamBID];

                if (result.Winner == MatchOutcome.WinA)
                {
                    teamA.TeamEntry.Points += 3;
                }
                else if (result.Winner == MatchOutcome.WinB)
                {
                    teamB.TeamEntry.Points += 3;
                }
                else // Draw
                {
                    teamA.TeamEntry.Points += 1;
                    teamB.TeamEntry.Points += 1;                    
                }

                BracketsMappers.AssignGoals(teamA.TeamEntry, teamB.TeamEntry, result);

                var groupResult = BracketsMappers.CreateGroupResult(result);
                groupResult.GroupCode = teamA.GroupCode;

                resultsList.Add(groupResult);
            }

            return resultsList;
        }                

        public static List<KnockoutMatchDTO> BuildRoundOf32(List<GroupTable> groups)
        {
            // Rank teams within each group using standard tie-breakers:
            // Points → Goal Difference → Goals Scored
            var ordered = groups.Select(g => new
            {
                g.GroupCode,
                Teams = g.Teams
                    .OrderByDescending(t => t.Points)
                    .ThenByDescending(t => t.GoalDifference)
                    .ThenByDescending(t => t.GoalsScored)
                    .ToList()
            }).ToList();

            // Extract top positions per group
            var first = ordered.ToDictionary(g => g.GroupCode, g => g.Teams[0]);
            var second = ordered.ToDictionary(g => g.GroupCode, g => g.Teams[1]);

            // Store third-place teams for later selection
            var thirdByGroup = ordered
                .ToDictionary(g => g.GroupCode, g => g.Teams[2]);

            // Select best third-placed teams across all groups using same tie-breakers
            var bestThirdGroups = ordered
                .Select(g => new { g.GroupCode, Team = g.Teams[2] })
                .OrderByDescending(t => t.Team.Points)
                .ThenByDescending(t => t.Team.GoalDifference)
                .ThenByDescending(t => t.Team.GoalsScored)
                .Take(8)
                .Select(t => t.GroupCode)
                .ToList();

            // Hardcoded bracket mapping based on tournament structure
            // Some slots are predefined (group winners and runners-up),
            // while others will be filled with best third-placed teams
            var matches = new List<KnockoutMatchDTO>
            {                
                new() { Key = 1, NextMatchKey = 1, TeamAID = first["E"].TeamId },
                new() { Key = 2, NextMatchKey = 1, TeamAID = first["I"].TeamId },
                new() { Key = 3, NextMatchKey = 2, TeamAID = second["A"].TeamId, TeamBID = second["B"].TeamId },
                new() { Key = 4, NextMatchKey = 2, TeamAID = first["F"].TeamId, TeamBID = second["C"].TeamId },
                new() { Key = 5, NextMatchKey = 3, TeamAID = second["K"].TeamId, TeamBID = second["L"].TeamId },
                new() { Key = 6, NextMatchKey = 3, TeamAID = first["H"].TeamId, TeamBID = second["J"].TeamId },
                new() { Key = 7, NextMatchKey = 4, TeamAID = first["D"].TeamId },
                new() { Key = 8, NextMatchKey = 4, TeamAID = first["G"].TeamId },
                new() { Key = 9, NextMatchKey = 5, TeamAID = first["C"].TeamId, TeamBID = second["F"].TeamId },
                new() { Key = 10, NextMatchKey = 5, TeamAID = second["E"].TeamId, TeamBID = second["I"].TeamId },
                new() { Key = 11, NextMatchKey = 6, TeamAID = first["A"].TeamId },
                new() { Key = 12, NextMatchKey = 6, TeamAID = first["L"].TeamId },
                new() { Key = 13, NextMatchKey = 7, TeamAID = first["J"].TeamId, TeamBID = second["H"].TeamId },
                new() { Key = 14, NextMatchKey = 7, TeamAID = second["D"].TeamId, TeamBID = second["G"].TeamId },
                new() { Key = 15, NextMatchKey = 8, TeamAID = first["B"].TeamId },
                new() { Key = 16, NextMatchKey = 8, TeamAID = first["K"].TeamId }
            };

            // Dynamically assign best third-placed teams to predefined bracket slots
            // based on tournament rules (mapping handled by AssignThirds)
            var assignedThirds = BracketsMappers.AssignThirds(bestThirdGroups);

            var matchByKey = matches.ToDictionary(m => m.Key);

            foreach (var third in assignedThirds)
            {
                var matchKey = third.Key;
                var groupCode = third.Value;
                var team = thirdByGroup[groupCode];                               
                var match = matchByKey[matchKey];

                // Safety check: ensure slot is not already filled
                if (match.TeamBID != null)
                    throw new Exception($"Match {matchKey} already has TeamB assigned.");

                match.TeamBID = team.TeamId;
            }

            return matches;
        }
    }
}
