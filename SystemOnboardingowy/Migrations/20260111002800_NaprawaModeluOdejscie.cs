using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemOnboardingowy.Migrations
{
    /// <inheritdoc />
    public partial class NaprawaModeluOdejscie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ZadaniaWdrozeniowe_Wdrozenia_WdrozenieId",
                table: "ZadaniaWdrozeniowe");

            migrationBuilder.AlterColumn<int>(
                name: "WdrozenieId",
                table: "ZadaniaWdrozeniowe",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "OdejscieId",
                table: "ZadaniaWdrozeniowe",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CzyZarchiwizowany",
                table: "Pracownicy",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "InstrukcjaDlaPlikow",
                table: "Odejscia",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_ZadaniaWdrozeniowe_OdejscieId",
                table: "ZadaniaWdrozeniowe",
                column: "OdejscieId");

            migrationBuilder.AddForeignKey(
                name: "FK_ZadaniaWdrozeniowe_Odejscia_OdejscieId",
                table: "ZadaniaWdrozeniowe",
                column: "OdejscieId",
                principalTable: "Odejscia",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ZadaniaWdrozeniowe_Wdrozenia_WdrozenieId",
                table: "ZadaniaWdrozeniowe",
                column: "WdrozenieId",
                principalTable: "Wdrozenia",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ZadaniaWdrozeniowe_Odejscia_OdejscieId",
                table: "ZadaniaWdrozeniowe");

            migrationBuilder.DropForeignKey(
                name: "FK_ZadaniaWdrozeniowe_Wdrozenia_WdrozenieId",
                table: "ZadaniaWdrozeniowe");

            migrationBuilder.DropIndex(
                name: "IX_ZadaniaWdrozeniowe_OdejscieId",
                table: "ZadaniaWdrozeniowe");

            migrationBuilder.DropColumn(
                name: "OdejscieId",
                table: "ZadaniaWdrozeniowe");

            migrationBuilder.DropColumn(
                name: "CzyZarchiwizowany",
                table: "Pracownicy");

            migrationBuilder.AlterColumn<int>(
                name: "WdrozenieId",
                table: "ZadaniaWdrozeniowe",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InstrukcjaDlaPlikow",
                table: "Odejscia",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ZadaniaWdrozeniowe_Wdrozenia_WdrozenieId",
                table: "ZadaniaWdrozeniowe",
                column: "WdrozenieId",
                principalTable: "Wdrozenia",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
