using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WCS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAccumulativeCountToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "AccumulatedCount",
                table: "NationalTeams",
                type: "integer",
                precision: 3,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldPrecision: 5,
                oldScale: 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "AccumulatedCount",
                table: "NationalTeams",
                type: "double precision",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldPrecision: 3);
        }
    }
}
