using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentroTerapia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFranjaExcepcion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pacientes_Familias_FamiliaId",
                table: "Pacientes");

            migrationBuilder.CreateTable(
                name: "FranjaExcepciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FranjaId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Motivo = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FranjaExcepciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FranjaExcepciones_FranjasDisponibilidad_FranjaId",
                        column: x => x.FranjaId,
                        principalTable: "FranjasDisponibilidad",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_FranjaExcepciones_FranjaId_Fecha",
                table: "FranjaExcepciones",
                columns: new[] { "FranjaId", "Fecha" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Pacientes_Familias_FamiliaId",
                table: "Pacientes",
                column: "FamiliaId",
                principalTable: "Familias",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pacientes_Familias_FamiliaId",
                table: "Pacientes");

            migrationBuilder.DropTable(
                name: "FranjaExcepciones");

            migrationBuilder.AddForeignKey(
                name: "FK_Pacientes_Familias_FamiliaId",
                table: "Pacientes",
                column: "FamiliaId",
                principalTable: "Familias",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
