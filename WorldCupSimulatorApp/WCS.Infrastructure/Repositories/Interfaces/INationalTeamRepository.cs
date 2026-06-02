using WCS.Application.DTO.RatingsDTO;

namespace WCS.Infrastructure.Repositories.Interfaces
{
    public interface INationalTeamRepository
    {

        // Bulk updates accumulated statistics for multiple teams.
        Task UpdateRatingsStatsBatchAsync(List<NationalTeamStatsUpdateDTO> updates);

        Task SaveAsync();
    }
}
