using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using WCS.Domain.Entities;
using WCS.Infrastructure.Persistence;

namespace WCS.Infrastructure.Data.Seeds
{
    public static class NationalTeamSeed
    {
        public static async Task SeedAsync(
            EFCoreDbContext db,
            string csvDirectory,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            if (await db.NationalTeams.AnyAsync(cancellationToken))
            {
                logger.LogInformation("NationalTeams already seeded. Skipping.");
                return;
            }

            var csvPath = Path.Combine(csvDirectory, "NationalTeams.csv");
            if (!File.Exists(csvPath))
            {
                logger.LogWarning("NationalTeams.csv not found at {Path}", csvPath);
                return;
            }

            logger.LogInformation("Seeding NationalTeams from {Path}", csvPath);

            using var reader = new StreamReader(csvPath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                Delimiter = ";"
            });

            csv.Context.RegisterClassMap<NationalTeamMap>();

            var records = csv.GetRecords<NationalTeam>().ToList();
            logger.LogInformation("Read {Count} NationalTeam records from CSV", records.Count);

            await db.NationalTeams.AddRangeAsync(records, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Successfully seeded {Count} NationalTeams", records.Count);
        }
    }

    public class NationalTeamMap : ClassMap<NationalTeam>
    {
        public NationalTeamMap()
        {
            Map(m => m.NationalTeamId).Name("NationalTeamId");
            Map(m => m.Name).Name("Name");
            Map(m => m.Code).Name("Code");
            Map(m => m.CurrentFifaRank).Name("CurrentFifaRank");
            Map(m => m.AccumulatedScores).Constant(0.0);
            Map(m => m.AccumulatedWeights).Constant(0.0);
            Map(m => m.AccumulatedPenalties).Constant(0.0);
            Map(m => m.AccumulatedCount).Constant(0);
            Map(m => m.AttackRating).Constant(0.0);
            Map(m => m.DefenseRating).Constant(0.0);
        }
    }
}