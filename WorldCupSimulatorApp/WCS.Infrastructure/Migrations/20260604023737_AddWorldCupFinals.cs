using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WCS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorldCupFinals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Points",
                table: "WorldCupTeams",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GoalsA",
                table: "WorldCupMatches",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GoalsB",
                table: "WorldCupMatches",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Played",
                table: "WorldCupMatches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "WorldCupFinals",
                columns: table => new
                {
                    WorldCupFinalsId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<int>(type: "integer", nullable: false),
                    Stage = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    NextMatchKey = table.Column<int>(type: "integer", nullable: false),
                    TeamAId = table.Column<int>(type: "integer", nullable: false),
                    TeamBId = table.Column<int>(type: "integer", nullable: false),
                    Played = table.Column<bool>(type: "boolean", nullable: false),
                    GoalsA = table.Column<int>(type: "integer", nullable: true),
                    GoalsB = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorldCupFinals", x => x.WorldCupFinalsId);
                    table.CheckConstraint("CK_WorldCupFinals_DifferentTeams", "\"TeamAId\" <> \"TeamBId\"");
                    table.CheckConstraint("CK_WorldCupFinals_GoalsA", "\"GoalsA\" >= 0");
                    table.CheckConstraint("CK_WorldCupFinals_GoalsB", "\"GoalsB\" >= 0");
                    table.CheckConstraint("CK_WorldCupFinals_Key", "\"Key\" BETWEEN 1 AND 16");
                    table.CheckConstraint("CK_WorldCupFinals_NextMatchKey", "\"NextMatchKey\" BETWEEN 1 AND 8");
                    table.ForeignKey(
                        name: "FK_WorldCupFinals_WorldCupTeams_TeamAId",
                        column: x => x.TeamAId,
                        principalTable: "WorldCupTeams",
                        principalColumn: "WorldCupTeamId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorldCupFinals_WorldCupTeams_TeamBId",
                        column: x => x.TeamBId,
                        principalTable: "WorldCupTeams",
                        principalColumn: "WorldCupTeamId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_WorldCupTeam_Points",
                table: "WorldCupTeams",
                sql: "\"Points\" BETWEEN 0 AND 9");

            migrationBuilder.AddCheckConstraint(
                name: "CK_WorldCupMatch_GoalsA",
                table: "WorldCupMatches",
                sql: "\"GoalsA\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_WorldCupMatch_GoalsB",
                table: "WorldCupMatches",
                sql: "\"GoalsB\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_WorldCupFinals_TeamAId",
                table: "WorldCupFinals",
                column: "TeamAId");

            migrationBuilder.CreateIndex(
                name: "IX_WorldCupFinals_TeamBId",
                table: "WorldCupFinals",
                column: "TeamBId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorldCupFinals");

            migrationBuilder.DropCheckConstraint(
                name: "CK_WorldCupTeam_Points",
                table: "WorldCupTeams");

            migrationBuilder.DropCheckConstraint(
                name: "CK_WorldCupMatch_GoalsA",
                table: "WorldCupMatches");

            migrationBuilder.DropCheckConstraint(
                name: "CK_WorldCupMatch_GoalsB",
                table: "WorldCupMatches");

            migrationBuilder.DropColumn(
                name: "Points",
                table: "WorldCupTeams");

            migrationBuilder.DropColumn(
                name: "GoalsA",
                table: "WorldCupMatches");

            migrationBuilder.DropColumn(
                name: "GoalsB",
                table: "WorldCupMatches");

            migrationBuilder.DropColumn(
                name: "Played",
                table: "WorldCupMatches");
        }
    }
}
