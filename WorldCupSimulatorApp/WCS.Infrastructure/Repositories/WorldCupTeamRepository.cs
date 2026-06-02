using Microsoft.EntityFrameworkCore;
using WCS.Application.DTO.BracketsDTO;
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
                    wt.PositionOrder,
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
                        PositionOrder = wt.PositionOrder
                    }).ToList()
                })
                .ToList();
        }
    }
}
