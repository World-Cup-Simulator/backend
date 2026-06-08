using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using WCS.Domain.Entities;
using WCS.Infrastructure.Persistence;

namespace WCS.Infrastructure.Data.Seeds
{
    public static class WorldCupMatchSeed
    {
        public static async Task SeedAsync(EFCoreDbContext db, string csvDirectory,
            ILogger logger, CancellationToken cancellationToken)
        {
            if (await db.WorldCupMatches.AnyAsync(cancellationToken))
            {
                logger.LogInformation("WorldCupMatches already seeded. Skipping.");
                return;
            }

            var csvPath = Path.Combine(csvDirectory, "WorldCupMatches.csv");
            if (!File.Exists(csvPath))
            {
                logger.LogWarning("WorldCupMatches.csv not found at {Path}", csvPath);
                return;
            }

            logger.LogInformation("Seeding WorldCupMatches from {Path}", csvPath);

            using var reader = new StreamReader(csvPath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                Delimiter = ";"
            });

            csv.Context.RegisterClassMap<WorldCupMatchMap>();

            var records = csv.GetRecords<WorldCupMatch>().ToList();
            logger.LogInformation("Read {Count} WorldCupMatch records from CSV", records.Count);

            await db.WorldCupMatches.AddRangeAsync(records, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Successfully seeded {Count} WorldCupMatches", records.Count);
        }
    }

    public class WorldCupMatchMap : ClassMap<WorldCupMatch>
    {
        public WorldCupMatchMap()
        {
            Map(m => m.WorldCupMatchId).Name("WorldCupMatchId");
            Map(m => m.Round).Name("Round");
            Map(m => m.Date).Name("Date").TypeConverterOption.Format("yyyy-MM-dd");
            Map(m => m.GroupCode).Name("GroupCode");
            Map(m => m.TeamAId).Name("TeamAId");
            Map(m => m.TeamBId).Name("TeamBId");
        }
    }
}