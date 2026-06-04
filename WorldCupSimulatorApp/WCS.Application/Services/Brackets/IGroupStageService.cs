using WCS.Application.DTO.BracketsDTO;
using WCS.Application.DTO.SimulatorsDTO;
using WCS.Domain.Entities;

namespace WCS.Application.Services.Brackets
{
    public interface IGroupStageService
    {
        List<GroupTable> BuildGroups(List<TeamGroupSummaryDTO> teams);

        List<GroupResultDTO> UpdateGroups(List<SimulationMatchDTO> simulationMatches, List<GroupTable> groupTables,
            Func<List<SimulationMatchDTO>, List<IMatchResult>> simulate);

        List<KnockoutMatchDTO> BuildRoundOf32(List<GroupTable> groups);
    }
}
