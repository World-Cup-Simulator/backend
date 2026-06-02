using WCS.Application.DTO.BracketsDTO;
using WCS.Application.DTO.SimulatorsDTO;
using WCS.Application.Mappers;
using WCS.Domain.Entities;
using WCS.Domain.Enums;

namespace WCS.Application.Services.Brackets
{
    public class GroupStageService
    {
        // Builds group tables from team summaries by grouping on GroupCode.
        public List<GroupTable> BuildGroups(List<TeamGroupSummaryDTO> teams)
        {
            ValidateTeams(teams);
            return teams
                .GroupBy(team => team.GroupCode)
                .OrderBy(group => group.Key)
                .Select(CreateGroupTable)
                .ToList();
        }

        // Runs group stage simulation and updates team standings.
        public List<GroupResultDTO> UpdateGroups(List<SimulationMatchDTO> simulationMatches, List<GroupTable> groupTables,
            Func<List<SimulationMatchDTO>, List<IMatchResult>> simulate)
        {
            ValidateInputs(simulationMatches, groupTables);

            // Execute simulation to get match outcomes
            var matchResults = simulate(simulationMatches);

            // Build lookup for O(1) team access during result processing

            var teamLookup = BuildTeamLookup(groupTables);

            // Process each result: update standings and build DTO
            return matchResults
                .Select(result => ProcessMatchResult(result, teamLookup))
                .ToList();
        }

        // Constructs the Round of 32 bracket from final group standings.
        public List<KnockoutMatchDTO> BuildRoundOf32(List<GroupTable> groups)
        {
            ValidateGroups(groups);

            // Convert group tables to ranked standings (1st, 2nd, 3rd, 4th per group)
            var groupRankings = RankGroupTables(groups);

            // Resolver handles bracket slot logic including third-place team assignment
            var resolver = new BracketSlotResolver(groupRankings);

            // Resolve each slot definition to a concrete match with teams assigned
            return BracketDefinitions.RoundOf32
                .Select(slot => resolver.Resolve(slot))
                .ToList();
        }

        private static GroupTable CreateGroupTable(IGrouping<string, TeamGroupSummaryDTO> group)
        {
            return new GroupTable
            {
                GroupCode = group.Key,
                Teams = group.Select(ToGroupTableEntry).ToList()
            };
        }

        private static GroupTableEntry ToGroupTableEntry(TeamGroupSummaryDTO team)
        {
            return new GroupTableEntry
            {
                TeamId = team.TeamId,
                Name = team.Name,
                FifaRank = team.FifaRank,
                AccumulatedScores = team.AccumulatedScores,
                AccumulatedWeights = team.AccumulatedWeights,
                AccumulatedPenalties = team.AccumulatedPenalties,
                AccumulatedCount = team.AccumulatedCount
            };
        }

        // Builds a lookup dictionary for O(1) access to teams by their TeamId.
        // 
        // WHY: During result processing, we need to quickly find both teams involved in each
        // match to update their standings. A dictionary eliminates O(n) searches through
        // nested group structures.
        // 
        // STRUCTURE: The dictionary maps TeamId -> Tuple(TeamEntry, GroupCode). The GroupCode
        // is stored alongside because it's needed for the GroupResultDTO but isn't on the
        // GroupTableEntry entity itself.
        private static Dictionary<int, (GroupTableEntry Entry, string GroupCode)> BuildTeamLookup(
            List<GroupTable> groupTables)
        {
            return groupTables
                .SelectMany(group => group.Teams.Select(team =>
                    new KeyValuePair<int, (GroupTableEntry, string)>(
                        team.TeamId, (team, group.GroupCode))))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }


        // Processes a single match result: updates team standings and constructs result DTO.
        private static GroupResultDTO ProcessMatchResult(IMatchResult result, Dictionary<int,
            (GroupTableEntry Entry, string GroupCode)> teamLookup)
        {
            var (teamA, groupCodeA) = teamLookup[result.TeamAID];
            var (teamB, groupCodeB) = teamLookup[result.TeamBID];

            // Updates Points and Goal statistics
            UpdateTeamStandings(teamA, teamB, result);

            var groupResult = GroupStageMappers.CreateGroupResult(result);

            // Enrich with group context and team metadata
            groupResult.GroupCode = groupCodeA;
            groupResult.TeamAFifaRank = teamA.FifaRank;
            groupResult.TeamBFifaRank = teamB.FifaRank;

            // Calculate derived attack ratings for adaptive simulation
            groupResult.AAttackRating = CalculateAttackRating(teamA);
            groupResult.BAttackRating = CalculateAttackRating(teamB);

            return groupResult;
        }

        private static void UpdateTeamStandings(
            GroupTableEntry teamA,
            GroupTableEntry teamB,
            IMatchResult result)
        {
            switch (result.Winner)
            {
                case MatchOutcome.WinA:
                    teamA.Points += 3;
                    break;
                case MatchOutcome.WinB:
                    teamB.Points += 3;
                    break;
                case MatchOutcome.Draw:
                    teamA.Points += 1;
                    teamB.Points += 1;
                    break;
            }
            GroupStageMappers.AssignGoals(teamA, teamB, result);
        }
        private static double CalculateAttackRating(GroupTableEntry team)
        {
            return team.AccumulatedWeights <= 0
                ? 0
                : team.AccumulatedScores / team.AccumulatedWeights;
        }

        // Ranks teams within each group using FIFA tournament tie-breaker rules.
        // It captures the final standings at a point in time for bracket construction.
        private static List<GroupRanking> RankGroupTables(List<GroupTable> groups)
        {
            return groups
                .Select(group => new GroupRanking(
                    group.GroupCode,
                    group.Teams
                        .OrderByDescending(team => team.Points)
                        .ThenByDescending(team => team.GoalDifference)
                        .ThenByDescending(team => team.GoalsScored)
                        .ToList()))
                .ToList();
        }

        private static void ValidateTeams(List<TeamGroupSummaryDTO> teams)
        {
            if (teams is null)
                throw new ArgumentNullException(nameof(teams));
            if (teams.Count == 0)
                throw new ArgumentException("Teams list cannot be empty.", nameof(teams));
        }

        private static void ValidateInputs(List<SimulationMatchDTO> matches, List<GroupTable> groupTables)
        {
            if (matches is null)
                throw new ArgumentNullException(nameof(matches));
            if (groupTables is null)
                throw new ArgumentNullException(nameof(groupTables));
            if (matches.Count == 0)
                throw new ArgumentException("Matches list cannot be empty.", nameof(matches));
            if (groupTables.Count == 0)
                throw new ArgumentException("Group tables list cannot be empty.", nameof(groupTables));
        }

        private static void ValidateGroups(List<GroupTable> groups)
        {
            if (groups is null)
                throw new ArgumentNullException(nameof(groups));
            if (groups.Count == 0)
                throw new ArgumentException("Groups list cannot be empty.", nameof(groups));
        }
    }
}
