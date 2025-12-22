using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentroTerapia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEspecialidadToTipoSesion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            try
            {
                migrationBuilder.AddColumn<int>(
                    name: "EspecialidadId",
                    table: "TiposSesion",
                    type: "int",
                    nullable: true);
            }
            catch { }

            try
            {
                migrationBuilder.CreateIndex(
                    name: "IX_TiposSesion_EspecialidadId",
                    table: "TiposSesion",
                    column: "EspecialidadId");
            }
            catch { }

            try
            {
                migrationBuilder.AddForeignKey(
                    name: "FK_TiposSesion_Especialidades_EspecialidadId",
                    table: "TiposSesion",
                    column: "EspecialidadId",
                    principalTable: "Especialidades",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            }
            catch { }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            try
            {
                migrationBuilder.DropForeignKey(
                    name: "FK_TiposSesion_Especialidades_EspecialidadId",
                    table: "TiposSesion");
            }
            catch { }

            try
            {
                migrationBuilder.DropIndex(
                    name: "IX_TiposSesion_EspecialidadId",
                    table: "TiposSesion");
            }
            catch { }

            try
            {
                migrationBuilder.DropColumn(
                    name: "EspecialidadId",
                    table: "TiposSesion");
            }
            catch { }
        }
    }
}
