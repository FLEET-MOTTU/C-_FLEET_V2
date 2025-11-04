using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Csharp.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFuncionarioSyncTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<short>(
                name: "Ativo",
                table: "Beacons",
                type: "NUMBER(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "BOOLEAN");

            migrationBuilder.CreateTable(
                name: "FUNCIONARIOS_SYNC",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    NOME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    EMAIL = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    TELEFONE = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    CARGO = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    STATUS = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    PATEO_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    FOTO_URL = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FUNCIONARIOS_SYNC", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FUNCIONARIOS_SYNC_EMAIL",
                table: "FUNCIONARIOS_SYNC",
                column: "EMAIL",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FUNCIONARIOS_SYNC_TELEFONE",
                table: "FUNCIONARIOS_SYNC",
                column: "TELEFONE",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FUNCIONARIOS_SYNC");

            migrationBuilder.AlterColumn<bool>(
                name: "Ativo",
                table: "Beacons",
                type: "BOOLEAN",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "NUMBER(1)");
        }
    }
}
