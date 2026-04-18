using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WCS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateWCSDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NationalTeams",
                columns: table => new
                {
                    NationalTeamId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Confederation = table.Column<int>(type: "integer", nullable: false),
                    CurrentFifaRank = table.Column<int>(type: "integer", nullable: false),
                    AttackRating = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: false),
                    DefenseRating = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: false),
                    OverallRating = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: false),
                    AvgGoalsScored = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: false),
                    AvgGoalsConceded = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NationalTeams", x => x.NationalTeamId);
                    table.CheckConstraint("CK_NationalTeam_AttackRating", "\"AttackRating\" >= 0");
                    table.CheckConstraint("CK_NationalTeam_AvgGoalsConceded", "\"AvgGoalsConceded\" >= 0");
                    table.CheckConstraint("CK_NationalTeam_AvgGoalsScored", "\"AvgGoalsScored\" >= 0");
                    table.CheckConstraint("CK_NationalTeam_Code_Length", "\"Code\" ~ '^[A-Z]{3}$'");
                    table.CheckConstraint("CK_NationalTeam_CurrentFifaRank", "\"CurrentFifaRank\" > 0");
                    table.CheckConstraint("CK_NationalTeam_DefenseRating", "\"DefenseRating\" >= 0");
                    table.CheckConstraint("CK_NationalTeam_OverallRating", "\"OverallRating\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "HistoricalMatches",
                columns: table => new
                {
                    HistoricalMatchId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    GoalsA = table.Column<int>(type: "integer", nullable: false),
                    GoalsB = table.Column<int>(type: "integer", nullable: false),
                    Competition = table.Column<int>(type: "integer", nullable: false),
                    Stage = table.Column<int>(type: "integer", nullable: false),
                    TeamAId = table.Column<int>(type: "integer", nullable: false),
                    TeamBId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricalMatches", x => x.HistoricalMatchId);
                    table.CheckConstraint("CK_HistoricalMatch_DifferentTeams", "\"TeamAId\" <> \"TeamBId\"");
                    table.CheckConstraint("CK_HistoricalMatch_GoalsA", "\"GoalsA\" >= 0");
                    table.CheckConstraint("CK_HistoricalMatch_GoalsB", "\"GoalsB\" >= 0");
                    table.ForeignKey(
                        name: "FK_HistoricalMatches_NationalTeams_TeamAId",
                        column: x => x.TeamAId,
                        principalTable: "NationalTeams",
                        principalColumn: "NationalTeamId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistoricalMatches_NationalTeams_TeamBId",
                        column: x => x.TeamBId,
                        principalTable: "NationalTeams",
                        principalColumn: "NationalTeamId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorldCupTeams",
                columns: table => new
                {
                    WorldCupTeamId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupCode = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    PositionOrder = table.Column<int>(type: "integer", nullable: false),
                    TeamId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorldCupTeams", x => x.WorldCupTeamId);
                    table.CheckConstraint("CK_WorldCupTeam_GroupCode", "\"GroupCode\" IN ('A','B','C','D','E','F','G','H','I','J','K','L')");
                    table.CheckConstraint("CK_WorldCupTeam_PositionOrder", "\"PositionOrder\" BETWEEN 1 AND 4");
                    table.ForeignKey(
                        name: "FK_WorldCupTeams_NationalTeams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "NationalTeams",
                        principalColumn: "NationalTeamId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorldCupMatches",
                columns: table => new
                {
                    WorldCupMatchId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Round = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    GroupCode = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    TeamAId = table.Column<int>(type: "integer", nullable: false),
                    TeamBId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorldCupMatches", x => x.WorldCupMatchId);
                    table.CheckConstraint("CK_WorldCupMatch_DifferentTeams", "\"TeamAId\" <> \"TeamBId\"");
                    table.CheckConstraint("CK_WorldCupMatch_Round", "\"Round\" BETWEEN 1 AND 3");
                    table.ForeignKey(
                        name: "FK_WorldCupMatches_WorldCupTeams_TeamAId",
                        column: x => x.TeamAId,
                        principalTable: "WorldCupTeams",
                        principalColumn: "WorldCupTeamId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorldCupMatches_WorldCupTeams_TeamBId",
                        column: x => x.TeamBId,
                        principalTable: "WorldCupTeams",
                        principalColumn: "WorldCupTeamId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoricalMatches_Date_TeamAId_TeamBId",
                table: "HistoricalMatches",
                columns: new[] { "Date", "TeamAId", "TeamBId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HistoricalMatches_TeamAId",
                table: "HistoricalMatches",
                column: "TeamAId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricalMatches_TeamBId",
                table: "HistoricalMatches",
                column: "TeamBId");

            migrationBuilder.CreateIndex(
                name: "IX_NationalTeams_Code",
                table: "NationalTeams",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorldCupMatches_Date_TeamAId_TeamBId",
                table: "WorldCupMatches",
                columns: new[] { "Date", "TeamAId", "TeamBId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorldCupMatches_TeamAId",
                table: "WorldCupMatches",
                column: "TeamAId");

            migrationBuilder.CreateIndex(
                name: "IX_WorldCupMatches_TeamBId",
                table: "WorldCupMatches",
                column: "TeamBId");

            migrationBuilder.CreateIndex(
                name: "IX_WorldCupTeams_GroupCode_PositionOrder",
                table: "WorldCupTeams",
                columns: new[] { "GroupCode", "PositionOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorldCupTeams_TeamId",
                table: "WorldCupTeams",
                column: "TeamId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoricalMatches");

            migrationBuilder.DropTable(
                name: "WorldCupMatches");

            migrationBuilder.DropTable(
                name: "WorldCupTeams");

            migrationBuilder.DropTable(
                name: "NationalTeams");
        }
    }
}
