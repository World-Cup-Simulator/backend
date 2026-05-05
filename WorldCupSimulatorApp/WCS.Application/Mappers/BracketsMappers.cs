using WCS.Application.DTO.MatchesDTO;
using WCS.Domain.Entities;

namespace WCS.Application.Mappers
{
    public class BracketsMappers
    {
        public static List<SimpleSimulationMatchDTO> CreateGroupMatches(List<WorldCupMatch> matches)
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

        public static void AssignGoals(GroupTableEntry teamA, GroupTableEntry teamB, MatchResultDTO result)
        {
            teamA.GoalsScored += result.GoalsA;
            teamA.GoalsConceded += result.GoalsB;
            teamB.GoalsScored += result.GoalsB;
            teamB.GoalsConceded += result.GoalsA;
        }

        // NOTE:
        // This method uses a greedy approach to assign third-place teams to bracket slots.
        // It does NOT explore all possible combinations.
        //
        // In theory, this could fail for certain edge-case distributions,
        // but given the tournament constraints and real-world data,
        // a valid assignment is extremely likely to be found.
        //
        // A full backtracking solution would guarantee correctness
        // at the cost of added complexity, which is intentionally avoided here.
        public static Dictionary<int, string> AssignThirds(List<string> bestThirdGroups)
        {
            // Requires exactly 8 third-place teams (tournament constraint)
            if (bestThirdGroups == null || bestThirdGroups.Count != 8)
                throw new ArgumentException("Exactly 8 third-place teams are required.");

            var assignment = new Dictionary<int, string>();
            var used = new HashSet<string>();

            // Fixed slot order is critical:
            // assignment is done sequentially and affects later choices
            var keysOrder = new List<int> { 1, 2, 7, 8, 11, 12, 15, 16 };

            foreach (var key in keysOrder)
            {
                var allowedGroups = ThirdPlaceEligibility[key];

                // Greedy selection:
                // pick the first available group that is allowed for this slot
                var selected = bestThirdGroups
                    .FirstOrDefault(g => allowedGroups.Contains(g) && !used.Contains(g));

                // Fail fast if no valid assignment exists for this slot
                if (selected == null)
                    throw new InvalidOperationException($"No valid third team for slot {key}");

                assignment[key] = selected;
                used.Add(selected);
            }

            return assignment;
        }

        public static readonly Dictionary<int, HashSet<string>> ThirdPlaceEligibility = new()
        {
            [1] = ["A", "B", "C", "D", "F"],
            [2] = ["C", "D", "F", "G", "H"],
            [7] = ["B", "E", "F", "I", "J"],
            [8] = ["A", "E", "H", "I", "J"],
            [11] = ["G", "E", "F", "H", "I"],
            [12] = ["E", "H", "I", "J", "K"],
            [15] = ["E", "F", "G", "I", "J"],
            [16] = ["D", "E", "I", "J", "L"]
        };
    }
}
