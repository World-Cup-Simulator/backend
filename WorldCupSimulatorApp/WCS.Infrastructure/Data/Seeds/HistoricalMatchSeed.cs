using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using WCS.Domain.Entities;
using WCS.Infrastructure.Persistence;

namespace WCS.Infrastructure.Data.Seeds
{
    public static class HistoricalMatchSeed
    {
        public static async Task SeedAsync(EFCoreDbContext db, string csvDirectory,
            ILogger logger, CancellationToken cancellationToken)
        {
            if (await db.HistoricalMatches.AnyAsync(cancellationToken))
            {
                logger.LogInformation("HistoricalMatches already seeded. Skipping.");
                return;
            }

            var csvPath = Path.Combine(csvDirectory, "HistoricalMatches.csv");
            if (!File.Exists(csvPath))
            {
                logger.LogWarning("HistoricalMatches.csv not found at {Path}", csvPath);
                return;
            }

            logger.LogInformation("Seeding HistoricalMatches from {Path}", csvPath);

            using var reader = new StreamReader(csvPath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                Delimiter = ";"
            });

            csv.Context.RegisterClassMap<HistoricalMatchMap>();

            var records = csv.GetRecords<HistoricalMatch>().ToList();
            logger.LogInformation("Read {Count} HistoricalMatch records from CSV", records.Count);

            await db.HistoricalMatches.AddRangeAsync(records, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Successfully seeded {Count} HistoricalMatches", records.Count);
        }
    }

    public class HistoricalMatchMap : ClassMap<HistoricalMatch>
    {
        public HistoricalMatchMap()
        {
            Map(m => m.HistoricalMatchId).Name("HistoricalMatchId");
            Map(m => m.Date).Name("Date").TypeConverterOption.Format("yyyy-MM-dd");
            Map(m => m.GoalsA).Name("GoalsA");
            Map(m => m.GoalsB).Name("GoalsB");
            Map(m => m.Competition).Index(4);
            Map(m => m.Stage).Index(5);
            Map(m => m.TeamAId).Name("TeamAId");
            Map(m => m.TeamBId).Name("TeamBId");
        }
    }
}