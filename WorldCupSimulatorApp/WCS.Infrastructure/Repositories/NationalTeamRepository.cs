using Microsoft.EntityFrameworkCore;
using WCS.Application.DTO.RatingsDTO;
using WCS.Infrastructure.Persistence;
using WCS.Infrastructure.Repositories.Interfaces;

namespace WCS.Infrastructure.Repositories
{
    public class NationalTeamRepository : INationalTeamRepository
    {
        private readonly EFCoreDbContext _dbContext;

        public NationalTeamRepository(EFCoreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task UpdateRatingsStatsBatchAsync(List<NationalTeamStatsUpdateDTO> updates)
        {
            var teamIds = updates.Select(u => u.TeamId).ToList();
            var teams = await _dbContext.NationalTeams
                .Where(nt => teamIds.Contains(nt.NationalTeamId))
                .ToDictionaryAsync(nt => nt.NationalTeamId);

            foreach (var update in updates)
            {
                if (teams.TryGetValue(update.TeamId, out var team))
                {
                    team.AttackRating = update.AttackRating;                    
                    team.AccumulatedScores = update.AccumulatedScores;
                    team.AccumulatedWeights = update.AccumulatedWeights;
                    team.DefenseRating = update.DefenseRating;
                    team.AccumulatedPenalties = update.AccumulatedPenalties;
                    team.AccumulatedCount = update.AccumulatedCount;
                }
            }
        }

        public async Task SaveAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
