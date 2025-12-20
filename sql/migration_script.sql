CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251220145319_InitialCreate') THEN

    ALTER DATABASE CHARACTER SET utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251220145319_InitialCreate') THEN

    CREATE TABLE `Familia` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `TelefonoContacto` longtext CHARACTER SET utf8mb4 NULL,
        `Responsable1Nombre` longtext CHARACTER SET utf8mb4 NULL,
        `Responsable1Apellido` longtext CHARACTER SET utf8mb4 NULL,
        `Responsable1DNI` longtext CHARACTER SET utf8mb4 NULL,
        `Responsable1Direccion` longtext CHARACTER SET utf8mb4 NULL,
        `Responsable1Email` longtext CHARACTER SET utf8mb4 NULL,
        `Responsable1Telefono` longtext CHARACTER SET utf8mb4 NULL,
        `Responsable1Relacion` longtext CHARACTER SET utf8mb4 NULL,
        `Responsable2Nombre` longtext CHARACTER SET utf8mb4 NULL,
        `Responsable2Apellido` longtext CHARACTER SET utf8mb4 NULL,
        `Responsable2DNI` longtext CHARACTER SET utf8mb4 NULL,
        `Responsable2Direccion` longtext CHARACTER SET utf8mb4 NULL,
        `Responsable2Email` longtext CHARACTER SET utf8mb4 NULL,
        `Responsable2Telefono` longtext CHARACTER SET utf8mb4 NULL,
        `Responsable2Relacion` longtext CHARACTER SET utf8mb4 NULL,
        `FechaCreacion` datetime(6) NOT NULL,
        `FechaActualizacion` datetime(6) NULL,
        CONSTRAINT `PK_Familia` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251220145319_InitialCreate') THEN

    CREATE TABLE `Terapeuta` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Nombres` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Apellidos` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Especialidades` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Presentacion` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Telefono` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Direccion` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Activo` tinyint(1) NOT NULL,
        `FechaCreacion` datetime(6) NOT NULL,
        CONSTRAINT `PK_Terapeuta` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251220145319_InitialCreate') THEN

    CREATE TABLE `TipoSesion` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Nombre` longtext CHARACTER SET utf8mb4 NOT NULL,
        `DuracionMinutos` int NOT NULL,
        `Precio` decimal(65,30) NULL,
        `Descripcion` longtext CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_TipoSesion` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251220145319_InitialCreate') THEN

    CREATE TABLE `Users` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Correo` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `HashContrasena` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Nombres` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Apellidos` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Rol` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `FechaCreacion` datetime(6) NOT NULL,
        `Activo` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Users` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251220145319_InitialCreate') THEN

    CREATE TABLE `Paciente` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Nombres` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Apellidos` longtext CHARACTER SET utf8mb4 NOT NULL,
        `FechaNacimiento` datetime(6) NOT NULL,
        `Sexo` longtext CHARACTER SET utf8mb4 NOT NULL,
        `FechaCreacion` datetime(6) NOT NULL,
        `NombreContactoEmergencia` longtext CHARACTER SET utf8mb4 NOT NULL,
        `NumeroContactoEmergencia` longtext CHARACTER SET utf8mb4 NOT NULL,
        `FamiliaId` int NOT NULL,
        CONSTRAINT `PK_Paciente` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Paciente_Familia_FamiliaId` FOREIGN KEY (`FamiliaId`) REFERENCES `Familia` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251220145319_InitialCreate') THEN

    CREATE TABLE `FranjaDisponibilidad` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `TerapeutaId` int NOT NULL,
        `Fecha` datetime(6) NULL,
        `DiaSemana` int NULL,
        `HoraInicio` time(6) NOT NULL,
        `HoraFin` time(6) NOT NULL,
        `Recurrente` tinyint(1) NOT NULL,
        CONSTRAINT `PK_FranjaDisponibilidad` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_FranjaDisponibilidad_Terapeuta_TerapeutaId` FOREIGN KEY (`TerapeutaId`) REFERENCES `Terapeuta` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251220145319_InitialCreate') THEN

    CREATE TABLE `Citas` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Fecha` datetime(6) NOT NULL,
        `Motivo` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Estado` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Notas` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `FechaCreacion` datetime(6) NOT NULL,
        `PacienteId` int NOT NULL,
        `TerapeutaId` int NULL,
        `TerapiaId` int NULL,
        `DuracionMinutos` int NOT NULL,
        `RowVersion` longblob NULL,
        CONSTRAINT `PK_Citas` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Citas_Paciente_PacienteId` FOREIGN KEY (`PacienteId`) REFERENCES `Paciente` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_Citas_Terapeuta_TerapeutaId` FOREIGN KEY (`TerapeutaId`) REFERENCES `Terapeuta` (`Id`),
        CONSTRAINT `FK_Citas_TipoSesion_TerapiaId` FOREIGN KEY (`TerapiaId`) REFERENCES `TipoSesion` (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251220145319_InitialCreate') THEN

    CREATE TABLE `NotaSesion` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `CitaId` int NOT NULL,
        `TerapeutaId` int NOT NULL,
        `Notas` longtext CHARACTER SET utf8mb4 NOT NULL,
        `FechaCreacion` datetime(6) NOT NULL,
        CONSTRAINT `PK_NotaSesion` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_NotaSesion_Citas_CitaId` FOREIGN KEY (`CitaId`) REFERENCES `Citas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_NotaSesion_Terapeuta_TerapeutaId` FOREIGN KEY (`TerapeutaId`) REFERENCES `Terapeuta` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251220145319_InitialCreate') THEN

    CREATE INDEX `IX_Citas_Estado` ON `Citas` (`Estado`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251220145319_InitialCreate') THEN

    CREATE INDEX `IX_Citas_Fecha` ON `Citas` (`Fecha`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251220145319_InitialCreate') THEN

    CREATE INDEX `IX_Citas_PacienteId` ON `Citas` (`PacienteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251220145319_InitialCreate') THEN

    CREATE INDEX `IX_Citas_TerapeutaId` ON `Citas` (`TerapeutaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251220145319_InitialCreate') THEN

    CREATE INDEX `IX_Citas_TerapiaId` ON `Citas` (`TerapiaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251220145319_InitialCreate') THEN

    CREATE INDEX `IX_FranjaDisponibilidad_TerapeutaId` ON `FranjaDisponibilidad` (`TerapeutaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251220145319_InitialCreate') THEN

    CREATE INDEX `IX_NotaSesion_CitaId` ON `NotaSesion` (`CitaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251220145319_InitialCreate') THEN

    CREATE INDEX `IX_NotaSesion_TerapeutaId` ON `NotaSesion` (`TerapeutaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251220145319_InitialCreate') THEN

    CREATE INDEX `IX_Paciente_FamiliaId` ON `Paciente` (`FamiliaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251220145319_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Users_Correo` ON `Users` (`Correo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251220145319_InitialCreate') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251220145319_InitialCreate', '8.0.2');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

