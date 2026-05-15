using WCS.Application.DTO.BracketsDTO;
using WCS.Application.DTO.MatchesDTO;
using WCS.Application.Mappers;
using WCS.Domain.Entities;
using WCS.Domain.Enums;

namespace WCS.Application.Services.Brackets
{
    public class GroupStageService
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
                        FifaRank = t.Team.CurrentFifaRank,
                        AccumulatedScores = t.Team.AccumulatedScores,
                        AccumulatedWeights = t.Team.AccumulatedWeights,
                        AccumulatedPenalties = t.Team.AccumulatedPenalties,
                        AccumulatedCount = t.Team.AccumulatedCount,
                    }).ToList()
                })
                .ToList();
        }

        public List<GroupResultDTO> UpdateGroups(List<WorldCupMatch> matches, List<GroupTable> groupsTable, Func<List<SimulationMatchDTO>, 
            List<IMatchResult>> simulate)
        {
            var resultsList = new List<GroupResultDTO>();

            var simulationList = GroupStageMappers.CreateGroupMatches(matches);

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

                GroupStageMappers.AssignGoals(teamA.TeamEntry, teamB.TeamEntry, result);

                var groupResult = GroupStageMappers.CreateGroupResult(result);
                groupResult.GroupCode = teamA.GroupCode;
                groupResult.TeamAFifaRank = teamA.TeamEntry.FifaRank;
                groupResult.TeamBFifaRank = teamB.TeamEntry.FifaRank;
                groupResult.AAttackRating = 
                    teamA.TeamEntry.AccumulatedWeights <= 0 ? 0 : teamA.TeamEntry.AccumulatedScores / teamA.TeamEntry.AccumulatedWeights;
                groupResult.BAttackRating =
                    teamB.TeamEntry.AccumulatedWeights <= 0 ? 0 : teamB.TeamEntry.AccumulatedScores / teamB.TeamEntry.AccumulatedWeights;

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
                new() { Key = 1, NextMatchKey = 1, TeamAID = first["E"].TeamId, AAccumulatedScores = first["E"].AccumulatedScores,
                AAccumulatedWeights = first["E"].AccumulatedWeights, AAccumulatedPenalties = first["E"].AccumulatedPenalties,
                AAccumulatedCount = first["E"].AccumulatedCount},

                new() { Key = 2, NextMatchKey = 1, TeamAID = first["I"].TeamId, AAccumulatedScores = first["I"].AccumulatedScores,
                AAccumulatedWeights = first["I"].AccumulatedWeights, AAccumulatedPenalties = first["I"].AccumulatedPenalties,
                AAccumulatedCount = first["I"].AccumulatedCount},

                new() { Key = 3, NextMatchKey = 2, TeamAID = second["A"].TeamId, AAccumulatedScores = second["A"].AccumulatedScores,
                AAccumulatedWeights = second["A"].AccumulatedWeights, AAccumulatedPenalties = second["A"].AccumulatedPenalties,
                AAccumulatedCount = second["A"].AccumulatedCount, TeamBID = second["B"].TeamId, BAccumulatedScores = second["B"].AccumulatedScores,
                BAccumulatedWeights = second["B"].AccumulatedWeights, BAccumulatedPenalties = second["B"].AccumulatedPenalties,
                BAccumulatedCount = second["B"].AccumulatedCount},

                new() { Key = 4, NextMatchKey = 2, TeamAID = first["F"].TeamId, AAccumulatedScores = first["F"].AccumulatedScores,
                AAccumulatedWeights = first["F"].AccumulatedWeights, AAccumulatedPenalties = first["F"].AccumulatedPenalties,
                AAccumulatedCount = first["F"].AccumulatedCount, TeamBID = second["C"].TeamId, BAccumulatedScores = second["C"].AccumulatedScores,
                BAccumulatedWeights = second["C"].AccumulatedWeights, BAccumulatedPenalties = second["C"].AccumulatedPenalties,
                BAccumulatedCount = second["C"].AccumulatedCount},

                new() { Key = 5, NextMatchKey = 3, TeamAID = second["K"].TeamId, AAccumulatedScores = second["K"].AccumulatedScores,
                AAccumulatedWeights = second["K"].AccumulatedWeights, AAccumulatedPenalties = second["K"].AccumulatedPenalties,
                AAccumulatedCount = second["K"].AccumulatedCount, TeamBID = second["L"].TeamId, BAccumulatedScores = second["L"].AccumulatedScores,
                BAccumulatedWeights = second["L"].AccumulatedWeights, BAccumulatedPenalties = second["L"].AccumulatedPenalties,
                BAccumulatedCount = second["L"].AccumulatedCount},

                new() { Key = 6, NextMatchKey = 3, TeamAID = first["H"].TeamId, AAccumulatedScores = first["H"].AccumulatedScores,
                AAccumulatedWeights = first["H"].AccumulatedWeights, AAccumulatedPenalties = first["H"].AccumulatedPenalties,
                AAccumulatedCount = first["H"].AccumulatedCount, TeamBID = second["J"].TeamId, BAccumulatedScores = second["J"].AccumulatedScores,
                BAccumulatedWeights = second["J"].AccumulatedWeights, BAccumulatedPenalties = second["J"].AccumulatedPenalties,
                BAccumulatedCount = second["J"].AccumulatedCount},

                new() { Key = 7, NextMatchKey = 4, TeamAID = first["D"].TeamId, AAccumulatedScores = first["D"].AccumulatedScores,
                AAccumulatedWeights = first["D"].AccumulatedWeights, AAccumulatedPenalties = first["D"].AccumulatedPenalties,
                AAccumulatedCount = first["D"].AccumulatedCount},

                new() { Key = 8, NextMatchKey = 4, TeamAID = first["G"].TeamId, AAccumulatedScores = first["G"].AccumulatedScores,
                AAccumulatedWeights = first["G"].AccumulatedWeights, AAccumulatedPenalties = first["G"].AccumulatedPenalties,
                AAccumulatedCount = first["G"].AccumulatedCount},

                new() { Key = 9, NextMatchKey = 5, TeamAID = first["C"].TeamId, AAccumulatedScores = first["C"].AccumulatedScores,
                AAccumulatedWeights = first["C"].AccumulatedWeights, AAccumulatedPenalties = first["C"].AccumulatedPenalties,
                AAccumulatedCount = first["C"].AccumulatedCount, TeamBID = second["F"].TeamId, BAccumulatedScores = second["F"].AccumulatedScores,
                BAccumulatedWeights = second["F"].AccumulatedWeights, BAccumulatedPenalties = second["F"].AccumulatedPenalties,
                BAccumulatedCount = second["F"].AccumulatedCount},

                new() { Key = 10, NextMatchKey = 5, TeamAID = second["E"].TeamId, AAccumulatedScores = second["E"].AccumulatedScores,
                AAccumulatedWeights = second["E"].AccumulatedWeights, AAccumulatedPenalties = second["E"].AccumulatedPenalties,
                AAccumulatedCount = second["E"].AccumulatedCount, TeamBID = second["I"].TeamId, BAccumulatedScores = second["I"].AccumulatedScores,
                BAccumulatedWeights = second["I"].AccumulatedWeights, BAccumulatedPenalties = second["I"].AccumulatedPenalties,
                BAccumulatedCount = second["I"].AccumulatedCount},

                new() { Key = 11, NextMatchKey = 6, TeamAID = first["A"].TeamId, AAccumulatedScores = first["A"].AccumulatedScores,
                AAccumulatedWeights = first["A"].AccumulatedWeights, AAccumulatedPenalties = first["A"].AccumulatedPenalties,
                AAccumulatedCount = first["A"].AccumulatedCount},

                new() { Key = 12, NextMatchKey = 6, TeamAID = first["L"].TeamId, AAccumulatedScores = first["L"].AccumulatedScores,
                AAccumulatedWeights = first["L"].AccumulatedWeights, AAccumulatedPenalties = first["L"].AccumulatedPenalties,
                AAccumulatedCount = first["L"].AccumulatedCount},

                new() { Key = 13, NextMatchKey = 7, TeamAID = first["J"].TeamId, AAccumulatedScores = first["J"].AccumulatedScores,
                AAccumulatedWeights = first["J"].AccumulatedWeights, AAccumulatedPenalties = first["J"].AccumulatedPenalties,
                AAccumulatedCount = first["J"].AccumulatedCount, TeamBID = second["H"].TeamId, BAccumulatedScores = second["H"].AccumulatedScores,
                BAccumulatedWeights = second["H"].AccumulatedWeights, BAccumulatedPenalties = second["H"].AccumulatedPenalties,
                BAccumulatedCount = second["H"].AccumulatedCount},

                new() { Key = 14, NextMatchKey = 7, TeamAID = second["D"].TeamId, AAccumulatedScores = second["D"].AccumulatedScores,
                AAccumulatedWeights = second["D"].AccumulatedWeights, AAccumulatedPenalties = second["D"].AccumulatedPenalties,
                AAccumulatedCount = second["D"].AccumulatedCount, TeamBID = second["G"].TeamId, BAccumulatedScores = second["G"].AccumulatedScores,
                BAccumulatedWeights = second["G"].AccumulatedWeights, BAccumulatedPenalties = second["G"].AccumulatedPenalties,
                BAccumulatedCount = second["G"].AccumulatedCount},

                new() { Key = 15, NextMatchKey = 8, TeamAID = first["B"].TeamId, AAccumulatedScores = first["B"].AccumulatedScores,
                AAccumulatedWeights = first["B"].AccumulatedWeights, AAccumulatedPenalties = first["B"].AccumulatedPenalties,
                AAccumulatedCount = first["B"].AccumulatedCount},

                new() { Key = 16, NextMatchKey = 8, TeamAID = first["K"].TeamId, AAccumulatedScores = first["K"].AccumulatedScores,
                AAccumulatedWeights = first["K"].AccumulatedWeights, AAccumulatedPenalties = first["K"].AccumulatedPenalties,
                AAccumulatedCount = first["K"].AccumulatedCount}
            };

            // Dynamically assign best third-placed teams to predefined bracket slots
            // based on tournament rules (mapping handled by AssignThirds)
            var assignedThirds = GroupStageMappers.AssignThirds(bestThirdGroups);

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
                match.BAccumulatedScores = team.AccumulatedScores;
                match.BAccumulatedWeights = team.AccumulatedWeights;
                match.BAccumulatedPenalties = team.AccumulatedPenalties;
                match.BAccumulatedCount = team.AccumulatedCount;

            }

            return matches;
        }
    }
}
