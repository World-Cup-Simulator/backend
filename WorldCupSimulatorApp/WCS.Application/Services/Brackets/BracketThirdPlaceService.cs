using WCS.Application.DTO.BracketsDTO;

namespace WCS.Application.Services.Brackets
{
    public class BracketThirdPlaceService : IBracketThirdPlaceService
    {
        // Matches with BestThirdSlot in order of their keys
        // These are the 8 matches where third-place teams can be assigned
        private static readonly int[] ThirdPlaceMatchKeys = { 1, 2, 7, 8, 11, 12, 15, 16 };

        public List<ThirdPlaceAssignmentDTO> AssignThirdPlaces(List<ThirdPlaceInputDTO> thirdPlaces)
        {
            if (thirdPlaces == null || thirdPlaces.Count != 8)
                throw new ArgumentException("Exactly 8 third-place teams required.", nameof(thirdPlaces));

            if (thirdPlaces.Select(t => t.Index).Distinct().Count() != 8)
                throw new ArgumentException("Indices must be unique (0-7).", nameof(thirdPlaces));

            // Build lookup: group -> index
            var groupToIndex = thirdPlaces.ToDictionary(
                t => t.Group.ToUpperInvariant(),
                t => t.Index);

            // Get third-place slots from bracket definitions
            var thirdPlaceSlots = GetThirdPlaceSlots();

            // Run backtracking to find valid assignment
            var assignments = new Dictionary<int, int>(); // matchKey -> index
            var availableGroups = new HashSet<string>(groupToIndex.Keys);

            if (!TryAssign(0, thirdPlaceSlots, groupToIndex, availableGroups, assignments))
            {
                var groups = string.Join(", ", thirdPlaces.Select(t => t.Group));
                throw new InvalidOperationException(
                    $"Unable to assign third-place teams to bracket slots. " +
                    $"The combination of groups ({groups}) creates an impossible bracket configuration.");
            }

            // Convert to DTOs
            return assignments
                .Select(a => new ThirdPlaceAssignmentDTO
                {
                    Key = a.Key,
                    Index = a.Value
                })
                .OrderBy(dto => ThirdPlaceMatchKeys.ToList().IndexOf(dto.Key))
                .ToList();
        }

        // Extracts the 8 third-place slots from bracket definitions with their eligibility.
        private List<ThirdPlaceSlotInfo> GetThirdPlaceSlots()
        {
            var slots = new List<ThirdPlaceSlotInfo>();

            foreach (var key in ThirdPlaceMatchKeys)
            {
                var slot = BracketDefinitions.RoundOf32.First(s => s.MatchKey == key);
                var bestThird = slot.HomeTeamSlot as BestThirdSlot
                    ?? slot.AwayTeamSlot as BestThirdSlot;

                if (bestThird == null)
                    throw new InvalidOperationException($"Match {key} does not have a BestThirdSlot");

                slots.Add(new ThirdPlaceSlotInfo(key, bestThird.EligibleGroups));
            }

            return slots;
        }

        // Backtracking algorithm to assign third-place teams to slots.
        private bool TryAssign(
            int slotIndex,
            List<ThirdPlaceSlotInfo> slots,
            Dictionary<string, int> groupToIndex,
            HashSet<string> availableGroups,
            Dictionary<int, int> assignments)
        {
            // Base case: all slots assigned
            if (slotIndex >= slots.Count)
                return true;

            var slot = slots[slotIndex];

            // Find eligible groups for this slot (intersection of eligible and available)
            var candidates = availableGroups
                .Where(g => slot.EligibleGroups.Contains(g))
                .ToList();

            foreach (var group in candidates)
            {
                // Assign this group to this slot
                assignments[slot.MatchKey] = groupToIndex[group];
                availableGroups.Remove(group);

                // Recursively try to assign remaining slots
                if (TryAssign(slotIndex + 1, slots, groupToIndex, availableGroups, assignments))
                    return true;

                // Backtrack
                availableGroups.Add(group);
                assignments.Remove(slot.MatchKey);
            }

            return false;
        }

        // Internal representation of a third-place slot requirement.
        private record ThirdPlaceSlotInfo(
            int MatchKey,
            string[] EligibleGroups
        );
    }
}
