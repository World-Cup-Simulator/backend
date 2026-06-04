using Microsoft.EntityFrameworkCore;
using System.Linq;
using WCS.Application.DTO.BracketsDTO;
using WCS.Application.DTO.DisplaysDTO;
using WCS.Application.DTO.UpdatesDTO;
using WCS.Infrastructure.Persistence;
using WCS.Infrastructure.Repositories.Interfaces;

namespace WCS.Infrastructure.Repositories
{
    public class WorldCupTeamRepository : IWorldCupTeamRepository
    {
        private readonly EFCoreDbContext _dbContext;

        public WorldCupTeamRepository(EFCoreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<int>> GetExistingIdsAsync(IEnumerable<int> ids)
        {
            return await _dbContext.WorldCupTeams
                .Where(t => ids.Contains(t.WorldCupTeamId))
                .Select(t => t.WorldCupTeamId)
                .ToListAsync();
        }

        public async Task<List<TeamGroupSummaryDTO>> GetAllForGroupStageAsync()
        {
            return await _dbContext.WorldCupTeams
                .AsNoTracking()
                .Include(wt => wt.Team)
                .Select(wt => new TeamGroupSummaryDTO(
                    wt.TeamId,
                    wt.Team.Name,
                    wt.Team.CurrentFifaRank,
                    wt.GroupCode,
                    wt.Points,
                    wt.Team.AccumulatedScores,
                    wt.Team.AccumulatedWeights,
                    wt.Team.AccumulatedPenalties,
                    wt.Team.AccumulatedCount
                ))
                .ToListAsync();
        }

        public async Task<List<GroupSeedDisplayDTO>> GetAllGroupsForDisplayAsync()
        {
            var teams = await _dbContext.WorldCupTeams
                .AsNoTracking()
                .Include(wt => wt.Team)
                .OrderBy(wt => wt.GroupCode)
                .ThenByDescending(wt => wt.Points)
                .ThenBy(wt => wt.PositionOrder)
                .ToListAsync();

            return teams
                .GroupBy(wt => wt.GroupCode)
                .Select(g => new GroupSeedDisplayDTO
                {
                    GroupCode = g.Key,
                    Teams = g.Select(wt => new SeededTeamDTO
                    {
                        TeamName = wt.Team.Name,
                        TeamCode = wt.Team.Code,
                        Points = wt.Points
                    }).ToList()
                })
                .ToList();
        }

        public async Task UpdatePointsBatchAsync(List<WorldCupTeamUpdateDTO> updates)
        {
            var teamIds = updates.Select(u => u.WorldCupTeamId).ToList();
            var teams = await _dbContext.WorldCupTeams
                .Where(m => teamIds.Contains(m.WorldCupTeamId))
                .ToDictionaryAsync(m => m.WorldCupTeamId);

            foreach (var update in updates)
            {
                if (teams.TryGetValue(update.WorldCupTeamId, out var team))
                {
                    team.Points = update.Points;
                }
            }
        }

        public async Task SaveAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
