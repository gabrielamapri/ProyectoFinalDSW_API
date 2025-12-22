using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentroTerapia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndex_Cita_Terapeuta_Fecha : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Citas_TerapeutaId_Fecha",
                table: "Citas",
                columns: new[] { "TerapeutaId", "Fecha" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Citas_TerapeutaId_Fecha",
                table: "Citas");
        }
    }
}
