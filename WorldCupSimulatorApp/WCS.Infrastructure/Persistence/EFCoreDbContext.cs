using Microsoft.EntityFrameworkCore;
using WCS.Domain.Entities;

public class EFCoreDbContext : DbContext
{
    public EFCoreDbContext(DbContextOptions<EFCoreDbContext> options)
        : base(options)
    {
    }

    public DbSet<NationalTeam> NationalTeams => Set<NationalTeam>();
    public DbSet<HistoricalMatch> HistoricalMatches => Set<HistoricalMatch>();
    public DbSet<WorldCupTeam> WorldCupTeams => Set<WorldCupTeam>();
    public DbSet<WorldCupMatch> WorldCupMatches => Set<WorldCupMatch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigureNationalTeam(modelBuilder);
        ConfigureHistoricalMatch(modelBuilder);
        ConfigureWorldCupTeam(modelBuilder);
        ConfigureWorldCupMatch(modelBuilder);
    }

    private static void ConfigureNationalTeam(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NationalTeam>(entity =>
        {
            entity.HasKey(x => x.NationalTeamId);

            entity.Property(x => x.Name)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Code)
                .HasMaxLength(3)
                .IsRequired();

            entity.HasIndex(x => x.Code)
                .IsUnique();

            entity.Property(x => x.AttackRating)
                .HasPrecision(5, 2);

            entity.Property(x => x.DefenseRating)
                .HasPrecision(5, 2);

            entity.Property(x => x.OverallRating)
                .HasPrecision(5, 2);

            entity.Property(x => x.AvgGoalsScored)
                .HasPrecision(5, 2);

            entity.Property(x => x.AvgGoalsConceded)
                .HasPrecision(5, 2);

            entity.HasMany(x => x.TeamAMatches)
                .WithOne(x => x.TeamA)
                .HasForeignKey(x => x.TeamAId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.TeamBMatches)
                .WithOne(x => x.TeamB)
                .HasForeignKey(x => x.TeamBId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "CK_NationalTeam_CurrentFifaRank",
                    "[CurrentFifaRank] > 0");

                t.HasCheckConstraint(
                    "CK_NationalTeam_AttackRating",
                    "[AttackRating] >= 0");

                t.HasCheckConstraint(
                    "CK_NationalTeam_DefenseRating",
                    "[DefenseRating] >= 0");

                t.HasCheckConstraint(
                    "CK_NationalTeam_OverallRating",
                    "[OverallRating] >= 0");

                t.HasCheckConstraint(
                    "CK_NationalTeam_AvgGoalsScored",
                    "[AvgGoalsScored] >= 0");

                t.HasCheckConstraint(
                    "CK_NationalTeam_AvgGoalsConceded",
                    "[AvgGoalsConceded] >= 0");

                t.HasCheckConstraint(
                    "CK_NationalTeam_Code_Length",
                    "LEN([Code]) = 3");
            });
        });
    }

    private static void ConfigureHistoricalMatch(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HistoricalMatch>(entity =>
        {
            entity.HasKey(x => x.HistoricalMatchId);

            entity.HasIndex(x => new
            {
                x.Date,
                x.TeamAId,
                x.TeamBId
            }).IsUnique();

            entity.HasOne(x => x.TeamA)
                .WithMany(x => x.TeamAMatches)
                .HasForeignKey(x => x.TeamAId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TeamB)
                .WithMany(x => x.TeamBMatches)
                .HasForeignKey(x => x.TeamBId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "CK_HistoricalMatch_DifferentTeams",
                    "[TeamAId] <> [TeamBId]");

                t.HasCheckConstraint(
                    "CK_HistoricalMatch_GoalsA",
                    "[GoalsA] >= 0");

                t.HasCheckConstraint(
                    "CK_HistoricalMatch_GoalsB",
                    "[GoalsB] >= 0");
            });
        });
    }

    private static void ConfigureWorldCupTeam(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorldCupTeam>(entity =>
        {
            entity.HasKey(x => x.WorldCupTeamId);

            entity.Property(x => x.GroupCode)
                .HasMaxLength(1)
                .IsRequired();

            entity.HasIndex(x => x.TeamId)
                .IsUnique();

            entity.HasIndex(x => new
            {
                x.GroupCode,
                x.PositionOrder
            }).IsUnique();

            entity.HasOne(x => x.Team)
                .WithMany()
                .HasForeignKey(x => x.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "CK_WorldCupTeam_PositionOrder",
                    "[PositionOrder] BETWEEN 1 AND 4");
            });
        });
    }

    private static void ConfigureWorldCupMatch(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorldCupMatch>(entity =>
        {
            entity.HasKey(x => x.WorldCupMatchId);

            entity.HasIndex(x => new
            {
                x.Date,
                x.TeamAId,
                x.TeamBId
            }).IsUnique();

            entity.HasOne(x => x.TeamA)
                .WithMany()
                .HasForeignKey(x => x.TeamAId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TeamB)
                .WithMany()
                .HasForeignKey(x => x.TeamBId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "CK_WorldCupMatch_DifferentTeams",
                    "[TeamAId] <> [TeamBId]");

                t.HasCheckConstraint(
                    "CK_WorldCupMatch_Round",
                    "[Round] BETWEEN 1 AND 3");
            });
        });
    }
}