using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Csharp.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate_AddMotoAndTagBleTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TagsBle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CodigoUnicoTag = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    NivelBateria = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagsBle", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Motos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Placa = table.Column<string>(type: "NVARCHAR2(8)", maxLength: 8, nullable: true),
                    Modelo = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    StatusMoto = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    DataCriacaoRegistro = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    DataRecolhimento = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    FuncionarioRecolhimentoId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    DataEntradaPatio = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    UltimoBeaconConhecidoId = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    UltimaVezVistoEmPatio = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    TagBleId = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Motos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Motos_TagsBle_TagBleId",
                        column: x => x.TagBleId,
                        principalTable: "TagsBle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Motos_TagBleId",
                table: "Motos",
                column: "TagBleId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Motos");

            migrationBuilder.DropTable(
                name: "TagsBle");
        }
    }
}
