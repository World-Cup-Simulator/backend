using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using WCS.Domain.Entities;
using WCS.Infrastructure.Persistence;

namespace WCS.Infrastructure.Data.Seeds
{
    public static class WorldCupTeamSeed
    {
        public static async Task SeedAsync(EFCoreDbContext db, string csvDirectory,
            ILogger logger, CancellationToken cancellationToken)
        {
            if (await db.WorldCupTeams.AnyAsync(cancellationToken))
            {
                logger.LogInformation("WorldCupTeams already seeded. Skipping.");
                return;
            }

            var csvPath = Path.Combine(csvDirectory, "WorldCupTeams.csv");
            if (!File.Exists(csvPath))
            {
                logger.LogWarning("WorldCupTeams.csv not found at {Path}", csvPath);
                return;
            }

            logger.LogInformation("Seeding WorldCupTeams from {Path}", csvPath);

            using var reader = new StreamReader(csvPath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                Delimiter = ";"
            });

            csv.Context.RegisterClassMap<WorldCupTeamMap>();

            var records = csv.GetRecords<WorldCupTeam>().ToList();
            logger.LogInformation("Read {Count} WorldCupTeam records from CSV", records.Count);

            await db.WorldCupTeams.AddRangeAsync(records, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Successfully seeded {Count} WorldCupTeams", records.Count);
        }
    }

    public class WorldCupTeamMap : ClassMap<WorldCupTeam>
    {
        public WorldCupTeamMap()
        {
            Map(m => m.WorldCupTeamId).Name("WorldCupTeamId");
            Map(m => m.GroupCode).Name("GroupCode");
            Map(m => m.PositionOrder).Name("PositionOrder");
            Map(m => m.PositionOrder).Name("PositionOrder");
            Map(m => m.TeamId).Name("TeamId");
        }
    }
}