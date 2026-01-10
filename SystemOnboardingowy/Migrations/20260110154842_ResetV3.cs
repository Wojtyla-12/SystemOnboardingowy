using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemOnboardingowy.Migrations
{
    /// <inheritdoc />
    public partial class ResetV3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Zadania_Wdrozenia_WdrozenieId",
                table: "Zadania");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Zadania",
                table: "Zadania");

            migrationBuilder.DropColumn(
                name: "Notatki",
                table: "Wdrozenia");

            migrationBuilder.DropColumn(
                name: "Komentarz",
                table: "Zadania");

            migrationBuilder.RenameTable(
                name: "Zadania",
                newName: "ZadaniaWdrozeniowe");

            migrationBuilder.RenameColumn(
                name: "DataRozpoczecia",
                table: "Wdrozenia",
                newName: "DataUtworzenia");

            migrationBuilder.RenameColumn(
                name: "TrescZadania",
                table: "ZadaniaWdrozeniowe",
                newName: "Tresc");

            migrationBuilder.RenameColumn(
                name: "DzialOdpowiedzialny",
                table: "ZadaniaWdrozeniowe",
                newName: "Dzial");

            migrationBuilder.RenameIndex(
                name: "IX_Zadania_WdrozenieId",
                table: "ZadaniaWdrozeniowe",
                newName: "IX_ZadaniaWdrozeniowe_WdrozenieId");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataRozpoczeciaPracy",
                table: "Wdrozenia",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_ZadaniaWdrozeniowe",
                table: "ZadaniaWdrozeniowe",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Odejscia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataUtworzenia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataOdejscia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CzyPrzekierowacPoczte = table.Column<bool>(type: "bit", nullable: false),
                    AdresatPrzekierowania = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InstrukcjaDlaPlikow = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PracownikId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Odejscia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Odejscia_Pracownicy_PracownikId",
                        column: x => x.PracownikId,
                        principalTable: "Pracownicy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notatki",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tresc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Autor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataUtworzenia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CzyAutomatyczna = table.Column<bool>(type: "bit", nullable: false),
                    WdrozenieId = table.Column<int>(type: "int", nullable: true),
                    OdejscieId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notatki", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notatki_Odejscia_OdejscieId",
                        column: x => x.OdejscieId,
                        principalTable: "Odejscia",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notatki_Wdrozenia_WdrozenieId",
                        column: x => x.WdrozenieId,
                        principalTable: "Wdrozenia",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notatki_OdejscieId",
                table: "Notatki",
                column: "OdejscieId");

            migrationBuilder.CreateIndex(
                name: "IX_Notatki_WdrozenieId",
                table: "Notatki",
                column: "WdrozenieId");

            migrationBuilder.CreateIndex(
                name: "IX_Odejscia_PracownikId",
                table: "Odejscia",
                column: "PracownikId");

            migrationBuilder.AddForeignKey(
                name: "FK_ZadaniaWdrozeniowe_Wdrozenia_WdrozenieId",
                table: "ZadaniaWdrozeniowe",
                column: "WdrozenieId",
                principalTable: "Wdrozenia",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ZadaniaWdrozeniowe_Wdrozenia_WdrozenieId",
                table: "ZadaniaWdrozeniowe");

            migrationBuilder.DropTable(
                name: "Notatki");

            migrationBuilder.DropTable(
                name: "Odejscia");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ZadaniaWdrozeniowe",
                table: "ZadaniaWdrozeniowe");

            migrationBuilder.DropColumn(
                name: "DataRozpoczeciaPracy",
                table: "Wdrozenia");

            migrationBuilder.RenameTable(
                name: "ZadaniaWdrozeniowe",
                newName: "Zadania");

            migrationBuilder.RenameColumn(
                name: "DataUtworzenia",
                table: "Wdrozenia",
                newName: "DataRozpoczecia");

            migrationBuilder.RenameColumn(
                name: "Tresc",
                table: "Zadania",
                newName: "TrescZadania");

            migrationBuilder.RenameColumn(
                name: "Dzial",
                table: "Zadania",
                newName: "DzialOdpowiedzialny");

            migrationBuilder.RenameIndex(
                name: "IX_ZadaniaWdrozeniowe_WdrozenieId",
                table: "Zadania",
                newName: "IX_Zadania_WdrozenieId");

            migrationBuilder.AddColumn<string>(
                name: "Notatki",
                table: "Wdrozenia",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Komentarz",
                table: "Zadania",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Zadania",
                table: "Zadania",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Zadania_Wdrozenia_WdrozenieId",
                table: "Zadania",
                column: "WdrozenieId",
                principalTable: "Wdrozenia",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
