using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Csharp.Api.Migrations
{
    /// <inheritdoc />
    public partial class NomeDescritivoParaSuaMudanca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TagsBle_CodigoUnicoTag",
                table: "TagsBle",
                column: "CodigoUnicoTag",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TagsBle_CodigoUnicoTag",
                table: "TagsBle");
        }
    }
}
