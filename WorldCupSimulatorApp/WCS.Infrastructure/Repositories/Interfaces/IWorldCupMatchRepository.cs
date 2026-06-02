using WCS.Application.DTO.BracketsDTO;
using WCS.Application.DTO.SimulatorsDTO;

namespace WCS.Infrastructure.Repositories.Interfaces
{
    public interface IWorldCupMatchRepository
    {
        // Retrieves all World Cup matches with accumulated stats for simulation.
        Task<List<SimulationMatchDTO>> GetAllForSimulationAsync();

        // Retrieves all World Cup matches ordered by date for frontend display.
        Task<List<WorldCupMatchDisplayDTO>> GetAllForDisplayAsync();

        // Retrieves matches filtered by group code, ordered by date.
        Task<List<WorldCupMatchDisplayDTO>> GetByGroupCodeAsync(string groupCode);
    }
}
