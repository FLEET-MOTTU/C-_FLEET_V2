using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Csharp.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBeaconsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Beacons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    BeaconId = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Ativo = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    UltimaZonaDetectada = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    UltimaVezVisto = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Beacons", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Beacons_BeaconId",
                table: "Beacons",
                column: "BeaconId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Beacons");
        }
    }
}