using WCS.Application.DTO.BracketsDTO;
using WCS.Domain.Entities;
using WCS.Domain.Enums;

namespace WCS.Application.Services.Brackets
{
    // Resolves bracket slot definitions into concrete KnockoutMatchDTO instances.
    // Handles third-place team assignment using a greedy strategy.
    public class BracketSlotResolver
    {
        private readonly IReadOnlyDictionary<string, GroupRanking> _rankings;
        private readonly HashSet<string> _qualifiedThirdGroups;
        private readonly HashSet<string> _assignedThirdGroups = new();

        public BracketSlotResolver(IEnumerable<GroupRanking> rankings)
        {
            _rankings = rankings.ToDictionary(r => r.GroupCode);

            // Select the 8 best third-placed teams across all groups
            _qualifiedThirdGroups = rankings
                .Select(r => new { r.GroupCode, ThirdPlace = r.RankedTeams[2] })
                .OrderByDescending(x => x.ThirdPlace.Points)
                .ThenByDescending(x => x.ThirdPlace.GoalDifference)
                .ThenByDescending(x => x.ThirdPlace.GoalsScored)
                .Take(8)
                .Select(x => x.GroupCode)
                .ToHashSet();
        }

        // Resolves a bracket slot into a concrete knockout match.
        public KnockoutMatchDTO Resolve(BracketSlot slot)
        {
            var match = new KnockoutMatchDTO
            {
                Key = slot.MatchKey,
                NextMatchKey = slot.NextMatchKey
            };
            ResolveTeamSlot(slot.HomeTeamSlot, match, isTeamA: true);
            ResolveTeamSlot(slot.AwayTeamSlot, match, isTeamA: false);
            return match;
        }
        private void ResolveTeamSlot(TeamSlot slot, KnockoutMatchDTO match, bool isTeamA)
        {
            switch (slot)
            {
                case GroupPositionSlot positionSlot:
                    var team = GetTeamFromPosition(positionSlot.GroupCode, positionSlot.Position);
                    PopulateTeamProperties(match, team, isTeamA);
                    break;
                case BestThirdSlot bestThirdSlot:
                    var thirdTeam = ResolveBestThirdTeam(bestThirdSlot.EligibleGroups)
                        ?? throw new InvalidOperationException(
                            $"No eligible third-place team found for match {match.Key}.");
                    PopulateTeamProperties(match, thirdTeam, isTeamA);
                    break;
                default:
                    throw new NotSupportedException($"Unknown team slot type: {slot.GetType().Name}");
            }
        }
        private GroupTableEntry GetTeamFromPosition(string groupCode, GroupPosition position)
        {
            if (!_rankings.TryGetValue(groupCode, out var ranking))
            {
                throw new InvalidOperationException($"Group '{groupCode}' not found in rankings.");
            }
            var index = position switch
            {
                GroupPosition.Winner => 0,
                GroupPosition.RunnerUp => 1,
                GroupPosition.ThirdPlace => 2,
                _ => throw new NotSupportedException($"Unknown position: {position}")
            };
            return ranking.RankedTeams[index];
        }
        private GroupTableEntry? ResolveBestThirdTeam(string[] eligibleGroups)
        {
            // Find the best available third-place team from eligible groups
            var candidates = eligibleGroups
                .Where(g => _qualifiedThirdGroups.Contains(g) && !_assignedThirdGroups.Contains(g))
                .Select(g => new { GroupCode = g, Team = _rankings[g].RankedTeams[2] })
                .OrderByDescending(x => x.Team.Points)
                .ThenByDescending(x => x.Team.GoalDifference)
                .ThenByDescending(x => x.Team.GoalsScored)
                .ToList();
            if (candidates.Count == 0)
                return null;
            var selected = candidates.First();
            _assignedThirdGroups.Add(selected.GroupCode);
            return selected.Team;
        }
        private static void PopulateTeamProperties(KnockoutMatchDTO match, GroupTableEntry team, bool isTeamA)
        {
            if (isTeamA)
            {
                match.TeamAID = team.TeamId;
                match.TeamA = team.Name;
                match.TeamAFifaRank = team.FifaRank;
                match.AAccumulatedScores = team.AccumulatedScores;
                match.AAccumulatedWeights = team.AccumulatedWeights;
                match.AAccumulatedPenalties = team.AccumulatedPenalties;
                match.AAccumulatedCount = team.AccumulatedCount;
            }
            else
            {
                match.TeamBID = team.TeamId;
                match.TeamB = team.Name;
                match.TeamBFifaRank = team.FifaRank;
                match.BAccumulatedScores = team.AccumulatedScores;
                match.BAccumulatedWeights = team.AccumulatedWeights;
                match.BAccumulatedPenalties = team.AccumulatedPenalties;
                match.BAccumulatedCount = team.AccumulatedCount;
            }
        }
    }

    // Represents the ranked teams within a single group.
    public record GroupRanking(
        string GroupCode,
        IReadOnlyList<GroupTableEntry> RankedTeams
    );
}
