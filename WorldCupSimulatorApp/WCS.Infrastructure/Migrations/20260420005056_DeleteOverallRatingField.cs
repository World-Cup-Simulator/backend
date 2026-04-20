using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WCS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DeleteOverallRatingField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_NationalTeam_OverallRating",
                table: "NationalTeams");

            migrationBuilder.DropColumn(
                name: "OverallRating",
                table: "NationalTeams");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "OverallRating",
                table: "NationalTeams",
                type: "double precision",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_NationalTeam_OverallRating",
                table: "NationalTeams",
                sql: "\"OverallRating\" >= 0");
        }
    }
}
