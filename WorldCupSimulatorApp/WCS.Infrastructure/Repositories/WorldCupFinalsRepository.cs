using Microsoft.EntityFrameworkCore;
using WCS.Application.DTO.BracketsDTO;
using WCS.Application.DTO.DisplaysDTO;
using WCS.Application.DTO.UpdatesDTO;
using WCS.Domain.Entities;
using WCS.Infrastructure.Persistence;
using WCS.Infrastructure.Repositories.Interfaces;

namespace WCS.Infrastructure.Repositories
{
    public class WorldCupFinalsRepository : IWorldCupFinalsRepository
    {
        private readonly EFCoreDbContext _dbContext;

        public WorldCupFinalsRepository(EFCoreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<int>> GetExistingIdsAsync(IEnumerable<int> ids)
        {
            return await _dbContext.WorldCupFinals
                .Where(t => ids.Contains(t.WorldCupFinalsId))
                .Select(t => t.WorldCupFinalsId)
                .ToListAsync();
        }

        public async Task<List<KnockoutMatchDTO>> GetAllForSimulationAsync()
        {
            return await _dbContext.WorldCupFinals
                .AsNoTracking()
                .Where(m => m.Played == false)
                .Include(m => m.TeamA)
                    .ThenInclude(wt => wt.Team)
                .Include(m => m.TeamB)
                    .ThenInclude(wt => wt.Team)
                .Select(m => new KnockoutMatchDTO
                {
                    Key = m.Key,
                    NextMatchKey = m.NextMatchKey,
                    TeamAID = m.TeamAId,
                    TeamA = m.TeamA.Team.Name,
                    TeamAFifaRank = m.TeamA.Team.CurrentFifaRank,
                    AAccumulatedScores = m.TeamA.Team.AccumulatedScores,
                    AAccumulatedWeights = m.TeamA.Team.AccumulatedWeights,
                    AAccumulatedPenalties = m.TeamA.Team.AccumulatedPenalties,
                    AAccumulatedCount = m.TeamA.Team.AccumulatedCount,
                    TeamBID = m.TeamBId,
                    TeamB = m.TeamB.Team.Name,
                    TeamBFifaRank = m.TeamB.Team.CurrentFifaRank,
                    BAccumulatedScores = m.TeamB.Team.AccumulatedScores,
                    BAccumulatedWeights = m.TeamB.Team.AccumulatedWeights,
                    BAccumulatedPenalties = m.TeamB.Team.AccumulatedPenalties,
                    BAccumulatedCount = m.TeamB.Team.AccumulatedCount
                })
                .ToListAsync();
        }

        public async Task<List<WorldCupFinalsDisplayDTO>> GetAllForDisplayAsync()
        {
            return await _dbContext.WorldCupFinals
                .AsNoTracking()
                .Include(m => m.TeamA)
                    .ThenInclude(wt => wt.Team)
                .Include(m => m.TeamB)
                    .ThenInclude(wt => wt.Team)
                .OrderBy(m => m.Key)
                .Select(m => new WorldCupFinalsDisplayDTO
                {
                    MatchId = m.WorldCupFinalsId,
                    Key = m.Key,
                    Stage = m.Stage,
                    Date = m.Date,
                    NextMatchKey = m.NextMatchKey,
                    TeamAName = m.TeamA.Team.Name,
                    TeamBName = m.TeamB.Team.Name,
                    TeamACode = m.TeamA.Team.Code,
                    TeamBCode = m.TeamB.Team.Code,
                    GoalsA = m.GoalsA,
                    GoalsB = m.GoalsB
                })
                .ToListAsync();
        }

        public async Task InsertListAsync(List<WorldCupFinals> finalsMatches)
        {
            if (finalsMatches == null || finalsMatches.Count == 0)
                return;

            await _dbContext.WorldCupFinals.AddRangeAsync(finalsMatches);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateScoresBatchAsync(List<WorldCupMatchUpdateDTO> updates)
        {
            var matchIds = updates.Select(u => u.WorldCupMatchId).ToList();
            var matches = await _dbContext.WorldCupFinals
                .Where(m => matchIds.Contains(m.WorldCupFinalsId))
                .ToDictionaryAsync(m => m.WorldCupFinalsId);

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
