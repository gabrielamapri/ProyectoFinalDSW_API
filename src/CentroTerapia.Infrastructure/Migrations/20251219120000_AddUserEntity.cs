using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentroTerapia.Infrastructure.Migrations
{
    public partial class AddUserEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use raw SQL with IF NOT EXISTS to be safe if table already exists
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `Users` (
                  `Id` int NOT NULL AUTO_INCREMENT,
                  `Correo` varchar(100) NOT NULL,
                  `HashContrasena` longtext NOT NULL,
                  `Nombres` varchar(50) NOT NULL,
                  `Apellidos` varchar(50) NOT NULL,
                  `Rol` varchar(50) NOT NULL,
                  `FechaCreacion` datetime(6) NOT NULL,
                  `Activo` tinyint(1) NOT NULL,
                  PRIMARY KEY (`Id`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

                CREATE UNIQUE INDEX IF NOT EXISTS `IX_Users_Correo` ON `Users` (`Correo`);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS `Users`;");
        }
    }
}
