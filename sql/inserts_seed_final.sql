
-- inserts_seed_final.sql
-- Ejecutar en la BD `CentroTerapiaDb`

USE CentroTerapiaDb;

START TRANSACTION;

-- 1) Usuarios (recepcionista + admin)
-- Los hashes bcrypt incluidos son válidos; si deseas cambiarlos, sustituye por nuevos hashes bcrypt.
-- Ejemplo C#: BCrypt.Net.BCrypt.HashPassword("TuPasswordSeguro123!")
INSERT INTO Users (Correo, HashContrasena, Nombres, Apellidos, Rol, FechaCreacion, Activo)
VALUES ('recepcion@centro.local', '$2a$11$938GXqrbVMMyWHvGFn5RAeakD.yPi/xLbtCE3AoLdYwG5ds6WiXWe', 'Recepcion', 'Centro', 'Recepcionista', NOW(), 1);
SET @userRecepcionId = LAST_INSERT_ID();

INSERT INTO Users (Correo, HashContrasena, Nombres, Apellidos, Rol, FechaCreacion, Activo)
VALUES ('admin@centro.local', '$2a$11$MSFHhJuQWAooQDCzfZKoNuCKgY/B/lr40SF8ZDsa9I72gW.ic.VJm', 'Admin', 'Sistema', 'Admin', NOW(), 1);
SET @userAdminId = LAST_INSERT_ID();

-- 2) Familias
INSERT INTO Familias (TelefonoContacto, Responsable1Nombre, Responsable1Apellido, Responsable1DNI, Responsable1Direccion, Responsable1Email, Responsable1Telefono, Responsable1Relacion, FechaCreacion)
VALUES ('987654321', 'María', 'Gómez', '12345678', 'Av. Siempre Viva 123', 'maria.gomez@email.com', '987654321', 'Madre', NOW());
SET @familiaId = LAST_INSERT_ID();

-- 3) Pacientes (asociado a la familia) — `Sexo` se guarda como entero (enum Sexo)
INSERT INTO Pacientes (Nombres, Apellidos, FechaNacimiento, Sexo, FechaCreacion, NombreContactoEmergencia, NumeroContactoEmergencia, FamiliaId)
VALUES ('Lucas', 'Gómez', '2018-05-15', 0, NOW(), 'María Gómez', '987654321', @familiaId);
SET @pacienteId = LAST_INSERT_ID();

-- 4) Especialidades (crear registros referenciables)
INSERT INTO Especialidades (Nombre, Descripcion, FechaCreacion)
VALUES ('Terapia del Lenguaje', 'Intervenciones para trastornos del lenguaje', NOW());
SET @esp1Id = LAST_INSERT_ID();

INSERT INTO Especialidades (Nombre, Descripcion, FechaCreacion)
VALUES ('Psicología Infantil', 'Atención psicológica infanto-juvenil', NOW());
SET @esp2Id = LAST_INSERT_ID();

INSERT INTO Especialidades (Nombre, Descripcion, FechaCreacion)
VALUES ('Fisioterapia Pediátrica', 'Fisioterapia orientada a niños', NOW());
SET @esp3Id = LAST_INSERT_ID();

-- 5) Terapeutas (insertar uno a uno para capturar IDs) — usar `EspecialidadId`
INSERT INTO Terapeutas (Nombres, Apellidos, EspecialidadId, Presentacion, Telefono, Direccion, Activo, FechaCreacion)
VALUES ('Ana', 'López', @esp1Id, 'Fonoaudióloga con 8 años de experiencia', '912345678', 'Calle Falsa 45', 1, NOW());
SET @terapeuta1Id = LAST_INSERT_ID();

INSERT INTO Terapeutas (Nombres, Apellidos, EspecialidadId, Presentacion, Telefono, Direccion, Activo, FechaCreacion)
VALUES ('Raúl', 'Pérez', @esp3Id, 'Fisioterapeuta pediátrico', '912345679', 'Calle Real 12', 1, NOW());
SET @terapeuta2Id = LAST_INSERT_ID();

-- 6) Tipos de sesión (terapias)
INSERT INTO TiposSesion (Nombre, DuracionMinutos, Precio, Descripcion)
VALUES ('Terapia del Lenguaje (Individual)', 45, 30.00, 'Mejora de lenguaje expresivo y comprensivo');
SET @tipo1Id = LAST_INSERT_ID();

INSERT INTO TiposSesion (Nombre, DuracionMinutos, Precio, Descripcion)
VALUES ('Estimulación Temprana', 60, 35.00, 'Intervención para bebés y niños con riesgo de retraso');
SET @tipo2Id = LAST_INSERT_ID();

-- 7) Franjas de disponibilidad (recurrentes por día de semana)
INSERT INTO FranjasDisponibilidad (TerapeutaId, Fecha, DiaSemana, HoraInicio, HoraFin, Recurrente)
VALUES
(@terapeuta1Id, NULL, 1, '09:00:00', '13:00:00', 1), -- Ana, lunes 09:00-13:00
(@terapeuta1Id, NULL, 3, '14:00:00', '18:00:00', 1), -- Ana, miércoles 14:00-18:00
(@terapeuta2Id, NULL, 2, '08:30:00', '12:30:00', 1); -- Raúl, martes 08:30-12:30

-- 8) Citas (una por una; TerapeutaId es nullable en la entidad)
INSERT INTO Citas (Fecha, Motivo, Estado, Notas, FechaCreacion, PacienteId, TerapeutaId, TerapiaId, DuracionMinutos)
VALUES ('2025-12-25 10:00:00', 'Evaluación inicial de lenguaje', 'Scheduled', 'Traer informes médicos', NOW(), @pacienteId, @terapeuta1Id, @tipo1Id, 45);
SET @cita1Id = LAST_INSERT_ID();

INSERT INTO Citas (Fecha, Motivo, Estado, Notas, FechaCreacion, PacienteId, TerapeutaId, TerapiaId, DuracionMinutos)
VALUES ('2025-12-26 09:30:00', 'Sesión de estimulación', 'Scheduled', NULL, NOW(), @pacienteId, @terapeuta2Id, @tipo2Id, 60);
SET @cita2Id = LAST_INSERT_ID();

-- 9) Nota de sesión (asociada a una cita)
INSERT INTO NotaSesion (CitaId, TerapeutaId, Notas, FechaCreacion)
VALUES (@cita1Id, @terapeuta1Id, 'Evaluación: articulación y comprensión dentro de límites esperados para la edad, recomienda 2 sesiones/semana.', NOW());

COMMIT;

-- Consultas rápidas para verificar
-- SELECT * FROM Users;
-- SELECT * FROM Familias;
-- SELECT * FROM Pacientes;
-- SELECT * FROM Terapeutas;
-- SELECT * FROM TiposSesion;
-- SELECT * FROM FranjasDisponibilidad;
-- SELECT * FROM Citas;
-- SELECT * FROM NotaSesion;
