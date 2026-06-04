using WCS.Application.DTO.UpdatesDTO;

namespace WCS.Infrastructure.Repositories.Interfaces
{
    public interface INationalTeamRepository
    {

        // Bulk updates accumulated statistics for multiple teams.
        Task UpdateRatingsStatsBatchAsync(List<NationalTeamStatsUpdateDTO> updates);

        Task<List<int>> GetExistingIdsAsync(IEnumerable<int> ids);

        Task SaveAsync();
    }
}
