using WCS.Application.DTO.BracketsDTO;
using WCS.Application.DTO.DisplaysDTO;
using WCS.Application.DTO.UpdatesDTO;

namespace WCS.Infrastructure.Repositories.Interfaces
{
    public interface IWorldCupTeamRepository
    {
        // Retrieves all World Cup teams with accumulated stats for group stage building.
        Task<List<TeamGroupSummaryDTO>> GetAllForGroupStageAsync();

        // Retrieves all groups with teams for frontend display (seeding information).
        Task<List<GroupSeedDisplayDTO>> GetAllGroupsForDisplayAsync();

        // Bulk updates points for multiple teams.
        Task UpdatePointsBatchAsync(List<WorldCupTeamUpdateDTO> updates);

        Task<List<int>> GetExistingIdsAsync(IEnumerable<int> ids);

        Task SaveAsync();
    }
}
