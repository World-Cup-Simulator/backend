using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WCS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRatingAccumulators : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_NationalTeam_AvgGoalsConceded",
                table: "NationalTeams");

            migrationBuilder.DropCheckConstraint(
                name: "CK_NationalTeam_AvgGoalsScored",
                table: "NationalTeams");

            migrationBuilder.RenameColumn(
                name: "AvgGoalsScored",
                table: "NationalTeams",
                newName: "AccumulatedWeights");

            migrationBuilder.RenameColumn(
                name: "AvgGoalsConceded",
                table: "NationalTeams",
                newName: "AccumulatedScores");

            migrationBuilder.AddColumn<double>(
                name: "AccumulatedCount",
                table: "NationalTeams",
                type: "double precision",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "AccumulatedPenalties",
                table: "NationalTeams",
                type: "double precision",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_NationalTeam_AccumulatedCount",
                table: "NationalTeams",
                sql: "\"AccumulatedCount\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_NationalTeam_AccumulatedPenalties",
                table: "NationalTeams",
                sql: "\"AccumulatedPenalties\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_NationalTeam_AccumulatedScores",
                table: "NationalTeams",
                sql: "\"AccumulatedScores\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_NationalTeam_AccumulatedWeights",
                table: "NationalTeams",
                sql: "\"AccumulatedWeights\" >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_NationalTeam_AccumulatedCount",
                table: "NationalTeams");

            migrationBuilder.DropCheckConstraint(
                name: "CK_NationalTeam_AccumulatedPenalties",
                table: "NationalTeams");

            migrationBuilder.DropCheckConstraint(
                name: "CK_NationalTeam_AccumulatedScores",
                table: "NationalTeams");

            migrationBuilder.DropCheckConstraint(
                name: "CK_NationalTeam_AccumulatedWeights",
                table: "NationalTeams");

            migrationBuilder.DropColumn(
                name: "AccumulatedCount",
                table: "NationalTeams");

            migrationBuilder.DropColumn(
                name: "AccumulatedPenalties",
                table: "NationalTeams");

            migrationBuilder.RenameColumn(
                name: "AccumulatedWeights",
                table: "NationalTeams",
                newName: "AvgGoalsScored");

            migrationBuilder.RenameColumn(
                name: "AccumulatedScores",
                table: "NationalTeams",
                newName: "AvgGoalsConceded");

            migrationBuilder.AddCheckConstraint(
                name: "CK_NationalTeam_AvgGoalsConceded",
                table: "NationalTeams",
                sql: "\"AvgGoalsConceded\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_NationalTeam_AvgGoalsScored",
                table: "NationalTeams",
                sql: "\"AvgGoalsScored\" >= 0");
        }
    }
}
