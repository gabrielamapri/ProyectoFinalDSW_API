using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentroTerapia.Infrastructure.src.CentroTerapia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Restore_Sexo_Especialidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Citas_Paciente_PacienteId",
                table: "Citas");

            migrationBuilder.DropForeignKey(
                name: "FK_Citas_Terapeuta_TerapeutaId",
                table: "Citas");

            migrationBuilder.DropForeignKey(
                name: "FK_FranjaDisponibilidad_Terapeuta_TerapeutaId",
                table: "FranjaDisponibilidad");

            migrationBuilder.DropForeignKey(
                name: "FK_NotaSesion_Terapeuta_TerapeutaId",
                table: "NotaSesion");

            migrationBuilder.DropForeignKey(
                name: "FK_Paciente_Familia_FamiliaId",
                table: "Paciente");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Terapeuta",
                table: "Terapeuta");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Paciente",
                table: "Paciente");

            migrationBuilder.DropColumn(
                name: "Especialidades",
                table: "Terapeuta");

            migrationBuilder.RenameTable(
                name: "Terapeuta",
                newName: "Terapeutas");

            migrationBuilder.RenameTable(
                name: "Paciente",
                newName: "Pacientes");

            migrationBuilder.RenameIndex(
                name: "IX_Paciente_FamiliaId",
                table: "Pacientes",
                newName: "IX_Pacientes_FamiliaId");

            migrationBuilder.AddColumn<int>(
                name: "EspecialidadId",
                table: "Terapeutas",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Sexo",
                table: "Pacientes",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Terapeutas",
                table: "Terapeutas",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pacientes",
                table: "Pacientes",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Especialidades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descripcion = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaCreacion = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Especialidades", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Terapeutas_EspecialidadId",
                table: "Terapeutas",
                column: "EspecialidadId");

            migrationBuilder.AddForeignKey(
                name: "FK_Citas_Pacientes_PacienteId",
                table: "Citas",
                column: "PacienteId",
                principalTable: "Pacientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Citas_Terapeutas_TerapeutaId",
                table: "Citas",
                column: "TerapeutaId",
                principalTable: "Terapeutas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FranjaDisponibilidad_Terapeutas_TerapeutaId",
                table: "FranjaDisponibilidad",
                column: "TerapeutaId",
                principalTable: "Terapeutas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NotaSesion_Terapeutas_TerapeutaId",
                table: "NotaSesion",
                column: "TerapeutaId",
                principalTable: "Terapeutas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pacientes_Familia_FamiliaId",
                table: "Pacientes",
                column: "FamiliaId",
                principalTable: "Familia",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Terapeutas_Especialidades_EspecialidadId",
                table: "Terapeutas",
                column: "EspecialidadId",
                principalTable: "Especialidades",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Citas_Pacientes_PacienteId",
                table: "Citas");

            migrationBuilder.DropForeignKey(
                name: "FK_Citas_Terapeutas_TerapeutaId",
                table: "Citas");

            migrationBuilder.DropForeignKey(
                name: "FK_FranjaDisponibilidad_Terapeutas_TerapeutaId",
                table: "FranjaDisponibilidad");

            migrationBuilder.DropForeignKey(
                name: "FK_NotaSesion_Terapeutas_TerapeutaId",
                table: "NotaSesion");

            migrationBuilder.DropForeignKey(
                name: "FK_Pacientes_Familia_FamiliaId",
                table: "Pacientes");

            migrationBuilder.DropForeignKey(
                name: "FK_Terapeutas_Especialidades_EspecialidadId",
                table: "Terapeutas");

            migrationBuilder.DropTable(
                name: "Especialidades");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Terapeutas",
                table: "Terapeutas");

            migrationBuilder.DropIndex(
                name: "IX_Terapeutas_EspecialidadId",
                table: "Terapeutas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pacientes",
                table: "Pacientes");

            migrationBuilder.DropColumn(
                name: "EspecialidadId",
                table: "Terapeutas");

            migrationBuilder.RenameTable(
                name: "Terapeutas",
                newName: "Terapeuta");

            migrationBuilder.RenameTable(
                name: "Pacientes",
                newName: "Paciente");

            migrationBuilder.RenameIndex(
                name: "IX_Pacientes_FamiliaId",
                table: "Paciente",
                newName: "IX_Paciente_FamiliaId");

            migrationBuilder.AddColumn<string>(
                name: "Especialidades",
                table: "Terapeuta",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Sexo",
                table: "Paciente",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Terapeuta",
                table: "Terapeuta",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Paciente",
                table: "Paciente",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Citas_Paciente_PacienteId",
                table: "Citas",
                column: "PacienteId",
                principalTable: "Paciente",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Citas_Terapeuta_TerapeutaId",
                table: "Citas",
                column: "TerapeutaId",
                principalTable: "Terapeuta",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FranjaDisponibilidad_Terapeuta_TerapeutaId",
                table: "FranjaDisponibilidad",
                column: "TerapeutaId",
                principalTable: "Terapeuta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NotaSesion_Terapeuta_TerapeutaId",
                table: "NotaSesion",
                column: "TerapeutaId",
                principalTable: "Terapeuta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Paciente_Familia_FamiliaId",
                table: "Paciente",
                column: "FamiliaId",
                principalTable: "Familia",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
