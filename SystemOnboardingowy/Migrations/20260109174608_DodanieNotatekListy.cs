using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemOnboardingowy.Migrations
{
    /// <inheritdoc />
    public partial class DodanieNotatekListy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notatki",
                table: "Wdrozenia",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notatki",
                table: "Wdrozenia");
        }
    }
}
