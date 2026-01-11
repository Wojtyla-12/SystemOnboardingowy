using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemOnboardingowy.Migrations
{
    /// <inheritdoc />
    public partial class ZmianaStanowiskaNaEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Stanowisko",
                table: "Pracownicy",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Stanowisko",
                table: "Pracownicy",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
