using WCS.Application.Services.Simulators;
using WCS.Domain.Entities;

namespace WCS.Application.Services.Brackets
{
    public class GroupStageService (ISimulationService simulationService)
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
    }
}
