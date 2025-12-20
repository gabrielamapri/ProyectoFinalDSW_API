-- Crea la tabla Especialidades usada por el seed
CREATE TABLE IF NOT EXISTS `Especialidades` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Nombre` longtext NOT NULL,
  `Descripcion` longtext NULL,
  `FechaCreacion` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
