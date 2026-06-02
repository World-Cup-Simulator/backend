using Microsoft.EntityFrameworkCore;
using WCS.Application.DTO.RatingsDTO;
using WCS.Infrastructure.Persistence;
using WCS.Infrastructure.Repositories.Interfaces;

namespace WCS.Infrastructure.Repositories
{
    public class HistoricalMatchRepository : IHistoricalMatchRepository
    {
        private readonly EFCoreDbContext _dbContext;

        public HistoricalMatchRepository(EFCoreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<RatingDataDTO>> GetAllForInitialRatingsAsync()
        {
            var matches = await _dbContext.HistoricalMatches
                .AsNoTracking()
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .ToListAsync();

            var ratingDataList = new List<RatingDataDTO>();

            foreach (var match in matches)
            {
                // Create RatingDataDTO from TeamA's perspective
                ratingDataList.Add(new RatingDataDTO
                {
                    TeamID = match.TeamAId,
                    GoalsScored = match.GoalsA,
                    GoalsConceded = match.GoalsB,
                    OpponentFifaRank = match.TeamB.CurrentFifaRank,
                    OpponentAttackRating = 0,
                    Date = match.Date,
                    Competition = match.Competition,
                    Stage = match.Stage
                });

                // Create RatingDataDTO from TeamB's perspective (inverse)
                ratingDataList.Add(new RatingDataDTO
                {
                    TeamID = match.TeamBId,
                    GoalsScored = match.GoalsB,
                    GoalsConceded = match.GoalsA,
                    OpponentFifaRank = match.TeamA.CurrentFifaRank,
                    OpponentAttackRating = 0,
                    Date = match.Date,
                    Competition = match.Competition,
                    Stage = match.Stage
                });
            }

            return ratingDataList;
        }
    }
}
