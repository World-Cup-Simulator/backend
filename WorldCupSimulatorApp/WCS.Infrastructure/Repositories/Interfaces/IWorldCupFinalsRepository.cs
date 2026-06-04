using WCS.Application.DTO.BracketsDTO;
using WCS.Application.DTO.DisplaysDTO;
using WCS.Application.DTO.UpdatesDTO;
using WCS.Domain.Entities;

namespace WCS.Infrastructure.Repositories.Interfaces
{
    public interface IWorldCupFinalsRepository
    {
        // Retrieves all World Cup finals matches with accumulated stats for simulation.
        Task<List<KnockoutMatchDTO>> GetAllForSimulationAsync();

        // Retrieves all World Cup finals matches ordered by key for frontend display.
        Task<List<WorldCupFinalsDisplayDTO>> GetAllForDisplayAsync();

        Task<List<int>> GetExistingIdsAsync(IEnumerable<int> ids);

        Task InsertListAsync(List<WorldCupFinals> finalsMatches);

        // Bulk updates score for multiple matches.
        Task UpdateScoresBatchAsync(List<WorldCupMatchUpdateDTO> updates);

        Task SaveAsync();

    }
}
