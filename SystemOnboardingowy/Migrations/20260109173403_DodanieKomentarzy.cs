using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemOnboardingowy.Migrations
{
    /// <inheritdoc />
    public partial class DodanieKomentarzy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Komentarz",
                table: "Zadania",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Komentarz",
                table: "Zadania");
        }
    }
}
