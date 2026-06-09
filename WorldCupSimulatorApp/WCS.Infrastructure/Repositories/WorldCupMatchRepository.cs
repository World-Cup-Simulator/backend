using Microsoft.EntityFrameworkCore;
using WCS.Application.DTO.DisplaysDTO;
using WCS.Application.DTO.SimulatorsDTO;
using WCS.Application.DTO.UpdatesDTO;
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

        public async Task<List<int>> GetExistingIdsAsync(IEnumerable<int> ids)
        {
            return await _dbContext.WorldCupMatches
                .Where(t => ids.Contains(t.WorldCupMatchId))
                .Select(t => t.WorldCupMatchId)
                .ToListAsync();
        }

        public async Task<List<SimulationMatchDTO>> GetAllForSimulationAsync()
        {
            return await _dbContext.WorldCupMatches
                .AsNoTracking()
                .Where(m => m.Played == false)
                .Include(m => m.TeamA)
                    .ThenInclude(wt => wt.Team)
                .Include(m => m.TeamB)
                    .ThenInclude(wt => wt.Team)
                .Select(m => new SimulationMatchDTO
                {
                    Date = m.Date,
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
                    TeamBCode = m.TeamB.Team.Code,
                    GoalsA = m.GoalsA,
                    GoalsB = m.GoalsB
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
                    TeamBCode = m.TeamB.Team.Code,
                    GoalsA = m.GoalsA,
                    GoalsB = m.GoalsB
                })
                .ToListAsync();
        }

        public async Task UpdateScoresBatchAsync(List<WorldCupMatchUpdateDTO> updates)
        {
            var matchIds = updates.Select(u => u.WorldCupMatchId).ToList();
            var matches = await _dbContext.WorldCupMatches
                .Where(m => matchIds.Contains(m.WorldCupMatchId))
                .ToDictionaryAsync(m => m.WorldCupMatchId);

            foreach (var update in updates)
            {
                if (matches.TryGetValue(update.WorldCupMatchId, out var match))
                {
                    match.Played = true;
                    match.GoalsA = update.GoalsA;
                    match.GoalsB = update.GoalsB;
                }
            }
        }

        public async Task SaveAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
