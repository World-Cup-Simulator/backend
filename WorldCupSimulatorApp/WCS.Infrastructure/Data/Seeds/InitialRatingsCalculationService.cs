using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WCS.Application.DTO.UpdatesDTO;
using WCS.Application.Services.Ratings;
using WCS.Infrastructure.Persistence;
using WCS.Infrastructure.Repositories.Interfaces;

namespace WCS.Infrastructure.Data.Seeds
{
    public class InitialRatingsCalculationService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<InitialRatingsCalculationService> _logger;

        public InitialRatingsCalculationService(IServiceProvider serviceProvider, ILogger<InitialRatingsCalculationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<EFCoreDbContext>();
            var ratingService = scope.ServiceProvider.GetRequiredService<IRatingService>();
            var matchRepo = scope.ServiceProvider.GetRequiredService<IHistoricalMatchRepository>();
            var teamRepo = scope.ServiceProvider.GetRequiredService<INationalTeamRepository>();

            // Check if data exists
            if (!await db.HistoricalMatches.AnyAsync(cancellationToken))
            {
                _logger.LogInformation("No historical matches found. Skipping initial ratings calculation.");
                return;
            }

            // Check if teams data exists
            if (!await db.NationalTeams.AnyAsync(cancellationToken))
            {
                _logger.LogInformation("No national teams found. Skipping initial ratings calculation.");
                return;
            }

            // Check if any team already has calculated attack ratings
            var hasCalculatedRatings = await db.NationalTeams
                .AsNoTracking()
                .AnyAsync(nt => nt.AttackRating > 0, cancellationToken);

            if (hasCalculatedRatings)
            {
                _logger.LogInformation("Ratings already calculated. Skipping initial ratings calculation.");
                return;
            }

            _logger.LogInformation("Starting initial ratings calculation...");

            // PASS 1: Calculate Attack Ratings
            _logger.LogInformation("Pass 1: Calculating attack ratings...");

            var ratingData = await matchRepo.GetAllForInitialRatingsAsync();
            var teamMatches = ratingData.GroupBy(r => r.TeamID);
            var attackUpdates = new List<NationalTeamStatsUpdateDTO>();

            foreach (var team in teamMatches)
            {
                var teamId = team.Key;
                var matches = team.ToList();

                var attackResult = ratingService.CalculateAttack(matches, 0, 0);

                attackUpdates.Add(new NationalTeamStatsUpdateDTO
                {
                    TeamId = teamId,
                    AttackRating = attackResult.AttackRating,
                    AccumulatedScores = attackResult.AccumulatedScores,
                    AccumulatedWeights = attackResult.AccumulatedWeights,
                    DefenseRating = 0,
                    AccumulatedPenalties = 0,
                    AccumulatedCount = 0
                });
            }

            await teamRepo.UpdateRatingsStatsBatchAsync(attackUpdates);
            await teamRepo.SaveAsync();
            _logger.LogInformation("Attack ratings calculated and saved for {Count} teams.", attackUpdates.Count);

            // PASS 2: Calculate Defense Ratings
            _logger.LogInformation("Pass 2: Calculating defense ratings...");

            // Reload matches - now with AttackRating populated
            var ratingDataWithAttack = await matchRepo.GetAllForInitialRatingsAsync();
            var teamGroupsWithAttack = ratingDataWithAttack.GroupBy(r => r.TeamID);
            var defenseUpdates = new List<NationalTeamStatsUpdateDTO>();

            foreach (var team in teamGroupsWithAttack)
            {
                var teamId = team.Key;
                var matches = team.ToList();

                var defenseResult = ratingService.CalculateDefense(matches, 0, 0);

                // Get existing attack values from first pass
                var existingAttack = attackUpdates.First(u => u.TeamId == teamId);

                defenseUpdates.Add(new NationalTeamStatsUpdateDTO
                {
                    TeamId = teamId,
                    AttackRating = existingAttack.AttackRating,
                    AccumulatedScores = existingAttack.AccumulatedScores,
                    AccumulatedWeights = existingAttack.AccumulatedWeights,
                    DefenseRating = defenseResult.DefenseRating,
                    AccumulatedPenalties = defenseResult.AccumulatedPenalties,
                    AccumulatedCount = defenseResult.AccumulatedCount
                });
            }

            await teamRepo.UpdateRatingsStatsBatchAsync(defenseUpdates);
            await teamRepo.SaveAsync();
            _logger.LogInformation("Defense ratings calculated and saved for {Count} teams.", defenseUpdates.Count);

            _logger.LogInformation("Initial ratings calculation completed successfully.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}