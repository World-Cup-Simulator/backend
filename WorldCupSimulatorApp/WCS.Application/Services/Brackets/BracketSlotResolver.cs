using WCS.Application.DTO.BracketsDTO;
using WCS.Domain.Entities;
using WCS.Domain.Enums;

namespace WCS.Application.Services.Brackets
{
    // Resolves bracket slot definitions into concrete KnockoutMatchDTO instances.
    // Handles third-place team assignment using backtracking algorithm.
    public class BracketSlotResolver
    {
        private readonly IReadOnlyDictionary<string, GroupRanking> _rankings;
        private readonly HashSet<string> _qualifiedThirdGroups;
        private readonly Dictionary<int, (string GroupCode, GroupTableEntry Team)> _thirdPlaceAssignments;
        private readonly List<ThirdPlaceRequirement> _thirdPlaceRequirements;

        // Internal representation of a third-place slot requirement
        private record ThirdPlaceRequirement(
            int MatchKey,
            string[] EligibleGroups,
            bool IsTeamA
        );

        public BracketSlotResolver(IEnumerable<GroupRanking> rankings)
        {
            _rankings = rankings.ToDictionary(r => r.GroupCode);
            _thirdPlaceAssignments = new Dictionary<int, (string GroupCode, GroupTableEntry Team)>();
            _thirdPlaceRequirements = new List<ThirdPlaceRequirement>();

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

        // Resolves all bracket slots into concrete knockout matches.
        public IEnumerable<KnockoutMatchDTO> ResolveAll(IEnumerable<BracketSlot> slots)
        {
            var slotList = slots.ToList();

            // Phase 1: Collect all third-place requirements
            CollectThirdPlaceRequirements(slotList);

            // Phase 2: Sort by constraint tightness (fewest eligible groups first) for optimization
            _thirdPlaceRequirements.Sort((a, b) => a.EligibleGroups.Length.CompareTo(b.EligibleGroups.Length));

            // Phase 3: Run backtracking algorithm to assign third-place teams
            var availableGroups = new HashSet<string>(_qualifiedThirdGroups);
            if (!TryAssignThirdPlaceTeams(0, availableGroups))
            {
                throw new InvalidOperationException(
                    "Unable to assign third-place teams to bracket slots. " +
                    "This occurs when group results create an impossible combination of " +
                    $"qualified third-place teams ({string.Join(", ", _qualifiedThirdGroups)}). " +
                    "Retry the simulation to get different group standings.");
            }

            // Phase 4: Resolve all slots into concrete matches
            return slotList.Select(Resolve);
        }

        // Collects all third-place slot requirements from bracket definitions
        private void CollectThirdPlaceRequirements(List<BracketSlot> slots)
        {
            foreach (var slot in slots)
            {
                if (slot.HomeTeamSlot is BestThirdSlot homeBestThird)
                {
                    _thirdPlaceRequirements.Add(new ThirdPlaceRequirement(
                        slot.MatchKey,
                        homeBestThird.EligibleGroups,
                        IsTeamA: true));
                }

                if (slot.AwayTeamSlot is BestThirdSlot awayBestThird)
                {
                    _thirdPlaceRequirements.Add(new ThirdPlaceRequirement(
                        slot.MatchKey,
                        awayBestThird.EligibleGroups,
                        IsTeamA: false));
                }
            }
        }

        // Backtracking algorithm to assign third-place teams to slots
        private bool TryAssignThirdPlaceTeams(int index, HashSet<string> availableGroups)
        {
            // Base case: all requirements assigned successfully
            if (index >= _thirdPlaceRequirements.Count)
                return true;

            var req = _thirdPlaceRequirements[index];

            var candidates = availableGroups.Intersect(req.EligibleGroups).ToList();

            // Try each available group that's eligible for this slot
            foreach (var group in candidates)
            {
                // Assign this group to this requirement
                _thirdPlaceAssignments[req.MatchKey] = (group, _rankings[group].RankedTeams[2]);
                availableGroups.Remove(group);

                // Recursively try to assign remaining requirements
                if (TryAssignThirdPlaceTeams(index + 1, availableGroups))
                    return true;

                // Backtrack: undo assignment and try next option
                availableGroups.Add(group);
                _thirdPlaceAssignments.Remove(req.MatchKey);
            }

            return false; // No valid assignment found for this branch
        }

        // Resolves a bracket slot into a concrete knockout match.
        private KnockoutMatchDTO Resolve(BracketSlot slot)
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
                case BestThirdSlot:
                    // Third-place team was pre-assigned during backtracking
                    var (groupCode, thirdTeam) = _thirdPlaceAssignments[match.Key];
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
