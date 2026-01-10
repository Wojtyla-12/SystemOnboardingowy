using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemOnboardingowy.Migrations
{
    /// <inheritdoc />
    public partial class NowePolaWdrozenia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdresyEmail",
                table: "Wdrozenia",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DostepDoDyskow",
                table: "Wdrozenia",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Stanowisko",
                table: "Wdrozenia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "WybranySprzet",
                table: "Wdrozenia",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WymaganyVPN",
                table: "Wdrozenia",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdresyEmail",
                table: "Wdrozenia");

            migrationBuilder.DropColumn(
                name: "DostepDoDyskow",
                table: "Wdrozenia");

            migrationBuilder.DropColumn(
                name: "Stanowisko",
                table: "Wdrozenia");

            migrationBuilder.DropColumn(
                name: "WybranySprzet",
                table: "Wdrozenia");

            migrationBuilder.DropColumn(
                name: "WymaganyVPN",
                table: "Wdrozenia");
        }
    }
}
