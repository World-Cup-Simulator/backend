using Microsoft.EntityFrameworkCore;
using WCS.Application.DTO.BracketsDTO;
using WCS.Application.DTO.SimulatorsDTO;
using WCS.Infrastructure.Persistence;
using WCS.Infrastructure.Repositories.Interfaces;

namespace WCS.Infrastructure.Repositories
{
    public class WorldCupMatchRepository : IWorldCupMatchRepository
    {
        private readonly EFCoreDbContext _dbContext;

        public WorldCupMatchRepository(EFCoreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<SimulationMatchDTO>> GetAllForSimulationAsync()
        {
            return await _dbContext.WorldCupMatches
                .AsNoTracking()
                .Include(m => m.TeamA)
                    .ThenInclude(wt => wt.Team)
                .Include(m => m.TeamB)
                    .ThenInclude(wt => wt.Team)
                .Select(m => new SimulationMatchDTO
                {
                    TeamAID = m.TeamAId,
                    TeamA = m.TeamA.Team.Name,
                    AAccumulatedScores = m.TeamA.Team.AccumulatedScores,
                    AAccumulatedWeights = m.TeamA.Team.AccumulatedWeights,
                    AAccumulatedPenalties = m.TeamA.Team.AccumulatedPenalties,
                    AAccumulatedCount = m.TeamA.Team.AccumulatedCount,
                    TeamBID = m.TeamBId,
                    TeamB = m.TeamB.Team.Name,
                    BAccumulatedScores = m.TeamB.Team.AccumulatedScores,
                    BAccumulatedWeights = m.TeamB.Team.AccumulatedWeights,
                    BAccumulatedPenalties = m.TeamB.Team.AccumulatedPenalties,
                    BAccumulatedCount = m.TeamB.Team.AccumulatedCount
                })
                .ToListAsync();
        }

        public async Task<List<WorldCupMatchDisplayDTO>> GetAllForDisplayAsync()
        {
            return await _dbContext.WorldCupMatches
                .AsNoTracking()
                .Include(m => m.TeamA)
                    .ThenInclude(wt => wt.Team)
                .Include(m => m.TeamB)
                    .ThenInclude(wt => wt.Team)
                .OrderBy(m => m.Date)
                .Select(m => new WorldCupMatchDisplayDTO
                {
                    MatchId = m.WorldCupMatchId,
                    Round = m.Round,
                    Date = m.Date,
                    GroupCode = m.GroupCode,
                    TeamAName = m.TeamA.Team.Name,
                    TeamBName = m.TeamB.Team.Name,
                    TeamACode = m.TeamA.Team.Code,
                    TeamBCode = m.TeamB.Team.Code
                })
                .ToListAsync();
        }

        public async Task<List<WorldCupMatchDisplayDTO>> GetByGroupCodeAsync(string groupCode)
        {
            return await _dbContext.WorldCupMatches
                .AsNoTracking()
                .Include(m => m.TeamA)
                    .ThenInclude(wt => wt.Team)
                .Include(m => m.TeamB)
                    .ThenInclude(wt => wt.Team)
                .Where(m => m.GroupCode == groupCode)
                .OrderBy(m => m.Date)
                .Select(m => new WorldCupMatchDisplayDTO
                {
                    MatchId = m.WorldCupMatchId,
                    Round = m.Round,
                    Date = m.Date,
                    GroupCode = m.GroupCode,
                    TeamAName = m.TeamA.Team.Name,
                    TeamBName = m.TeamB.Team.Name,
                    TeamACode = m.TeamA.Team.Code,
                    TeamBCode = m.TeamB.Team.Code
                })
                .ToListAsync();
        }
    }
}
