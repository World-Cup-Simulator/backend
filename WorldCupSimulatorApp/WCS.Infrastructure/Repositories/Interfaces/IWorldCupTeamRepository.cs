using WCS.Application.DTO.BracketsDTO;
using WCS.Application.DTO.DisplaysDTO;

namespace WCS.Infrastructure.Repositories.Interfaces
{
    public interface IWorldCupTeamRepository
    {
        // Retrieves all World Cup teams with accumulated stats for group stage building.
        Task<List<TeamGroupSummaryDTO>> GetAllForGroupStageAsync();

        // Retrieves all groups with teams for frontend display (seeding information).
        Task<List<GroupSeedDisplayDTO>> GetAllGroupsForDisplayAsync();
    }
}
