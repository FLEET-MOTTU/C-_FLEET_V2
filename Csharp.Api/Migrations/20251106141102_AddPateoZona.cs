using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Csharp.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPateoZona : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PATEOS_SYNC",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    NOME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    PLANTA_BAIXA_URL = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    PLANTA_LARGURA = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    PLANTA_ALTURA = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    GERENCIADO_POR_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    STATUS = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PATEOS_SYNC", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ZONAS_SYNC",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    NOME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    PATEO_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CRIADO_POR_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    COORDENADAS_WKT = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZONAS_SYNC", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ZONAS_SYNC_PATEOS_SYNC_PATEO_ID",
                        column: x => x.PATEO_ID,
                        principalTable: "PATEOS_SYNC",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FUNCIONARIOS_SYNC_PATEO_ID",
                table: "FUNCIONARIOS_SYNC",
                column: "PATEO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ZONAS_SYNC_PATEO_ID",
                table: "ZONAS_SYNC",
                column: "PATEO_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_FUNCIONARIOS_SYNC_PATEOS_SYNC_PATEO_ID",
                table: "FUNCIONARIOS_SYNC",
                column: "PATEO_ID",
                principalTable: "PATEOS_SYNC",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FUNCIONARIOS_SYNC_PATEOS_SYNC_PATEO_ID",
                table: "FUNCIONARIOS_SYNC");

            migrationBuilder.DropTable(
                name: "ZONAS_SYNC");

            migrationBuilder.DropTable(
                name: "PATEOS_SYNC");

            migrationBuilder.DropIndex(
                name: "IX_FUNCIONARIOS_SYNC_PATEO_ID",
                table: "FUNCIONARIOS_SYNC");
        }
    }
}
