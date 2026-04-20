using Microsoft.EntityFrameworkCore;
using WCS.Domain.Entities;

namespace WCS.Infrastructure.Persistence
{
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

        private void ConfigureNationalTeam(ModelBuilder modelBuilder)
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

                entity.Property(x => x.AvgGoalsScored)
                    .HasPrecision(5, 2);

                entity.Property(x => x.AvgGoalsConceded)
                    .HasPrecision(5, 2);

                entity.HasMany(t => t.TeamAMatches)
                    .WithOne(hm => hm.TeamA)
                    .HasForeignKey(hm => hm.TeamAId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(t => t.TeamBMatches)
                    .WithOne(hm => hm.TeamB)
                    .HasForeignKey(hm => hm.TeamBId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint(
                        "CK_NationalTeam_CurrentFifaRank",
                        "\"CurrentFifaRank\" > 0");

                    t.HasCheckConstraint(
                        "CK_NationalTeam_AttackRating",
                        "\"AttackRating\" >= 0");

                    t.HasCheckConstraint(
                        "CK_NationalTeam_DefenseRating",
                        "\"DefenseRating\" >= 0");

                    t.HasCheckConstraint(
                        "CK_NationalTeam_AvgGoalsScored",
                        "\"AvgGoalsScored\" >= 0");

                    t.HasCheckConstraint(
                        "CK_NationalTeam_AvgGoalsConceded",
                        "\"AvgGoalsConceded\" >= 0");

                    t.HasCheckConstraint(
                        "CK_NationalTeam_Code_Length",
                        "\"Code\" ~ '^[A-Z]{3}$'");
                });
            });
        }

        private void ConfigureHistoricalMatch(ModelBuilder modelBuilder)
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

                entity.HasOne(hm =>  hm.TeamA)
                    .WithMany(t => t.TeamAMatches)
                    .HasForeignKey(hm => hm.TeamAId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(hm => hm.TeamB)
                    .WithMany(t => t.TeamBMatches)
                    .HasForeignKey(hm => hm.TeamBId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint(
                        "CK_HistoricalMatch_DifferentTeams",
                        "\"TeamAId\" <> \"TeamBId\"");

                    t.HasCheckConstraint(
                        "CK_HistoricalMatch_GoalsA",
                        "\"GoalsA\" >= 0");

                    t.HasCheckConstraint(
                        "CK_HistoricalMatch_GoalsB",
                        "\"GoalsB\" >= 0");
                });
            });
        }

        private void ConfigureWorldCupTeam(ModelBuilder modelBuilder)
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

                entity.HasOne(wct => wct.Team)
                    .WithOne(t => t.WorldCupTeam)
                    .HasForeignKey<WorldCupTeam>(wct => wct.TeamId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(wct => wct.TeamAMatches)
                    .WithOne(wcm => wcm.TeamA)
                    .HasForeignKey(wcm => wcm.TeamAId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(wct => wct.TeamBMatches)
                    .WithOne(wcm => wcm.TeamB)
                    .HasForeignKey(wcm => wcm.TeamBId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint(
                        "CK_WorldCupTeam_PositionOrder",
                        "\"PositionOrder\" BETWEEN 1 AND 4");

                    t.HasCheckConstraint(
                        "CK_WorldCupTeam_GroupCode",
                        "\"GroupCode\" IN ('A','B','C','D','E','F','G','H','I','J','K','L')");
                    });
            });
        }

        private void ConfigureWorldCupMatch(ModelBuilder modelBuilder)
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

                entity.HasOne(wcm => wcm.TeamA)
                    .WithMany(wct => wct.TeamAMatches)
                    .HasForeignKey(wcm => wcm.TeamAId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(wcm => wcm.TeamB)
                    .WithMany(wct => wct.TeamBMatches)
                    .HasForeignKey(wcm => wcm.TeamBId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint(
                        "CK_WorldCupMatch_DifferentTeams",
                        "\"TeamAId\" <> \"TeamBId\"");

                    t.HasCheckConstraint(
                        "CK_WorldCupMatch_Round",
                        "\"Round\" BETWEEN 1 AND 3");
                });
            });
        }
    }
}