using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WCS.Infrastructure.Persistence;

namespace WCS.Infrastructure.Data.Seeds
{
    public class CsvSeedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CsvSeedService> _logger;

        public CsvSeedService(
            IServiceProvider serviceProvider,
            ILogger<CsvSeedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EFCoreDbContext>();

            _logger.LogInformation("Applying database migrations...");
            await db.Database.MigrateAsync(cancellationToken);

            var infrastructureAssembly = typeof(CsvSeedService).Assembly;
            var infrastructureAssemblyPath = infrastructureAssembly.Location;
            var infrastructureAssemblyDir = Path.GetDirectoryName(infrastructureAssemblyPath)!;
            var csvDirectory = Path.Combine(infrastructureAssemblyDir, "Data", "CSV");
            _logger.LogInformation("Using CSV directory: {Directory}", csvDirectory);

            try
            {
                await NationalTeamSeed.SeedAsync(db, csvDirectory, _logger, cancellationToken);
                await HistoricalMatchSeed.SeedAsync(db, csvDirectory, _logger, cancellationToken);
                await WorldCupTeamSeed.SeedAsync(db, csvDirectory, _logger, cancellationToken);
                await WorldCupMatchSeed.SeedAsync(db, csvDirectory, _logger, cancellationToken);

                _logger.LogInformation("Database seeding completed successfully.");                              
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database seeding: {Message}", ex.Message);
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}