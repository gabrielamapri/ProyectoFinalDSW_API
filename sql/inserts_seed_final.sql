
-- inserts_seed_final.sql
-- Ejecutar en la BD `CentroTerapiaDb`

USE CentroTerapiaDb;

START TRANSACTION;

-- 1) Usuarios (recepcionista + admin)
-- Los hashes bcrypt incluidos son válidos; si deseas cambiarlos, sustituye por nuevos hashes bcrypt.
-- Ejemplo C#: BCrypt.Net.BCrypt.HashPassword("TuPasswordSeguro123!")

-- Admin
INSERT INTO Users (Correo, HashContrasena, Nombres, Apellidos, Rol, FechaCreacion, Activo)
VALUES ('admin@centro.local', '$2a$11$MSFHhJuQWAooQDCzfZKoNuCKgY/B/lr40SF8ZDsa9I72gW.ic.VJm', 'Admin', 'Sistema', 'Admin', NOW(), 1);

-- Terapeuta
INSERT INTO Users (Correo, HashContrasena, Nombres, Apellidos, Rol, FechaCreacion, Activo)
VALUES ('terapeuta@centro.local', '$2a$11$MSFHhJuQWAooQDCzfZKoNuCKgY/B/lr40SF8ZDsa9I72gW.ic.VJm', 'Terapeuta', 'Ejemplo', 'Terapeuta', NOW(), 1);

-- Padre
INSERT INTO Users (Correo, HashContrasena, Nombres, Apellidos, Rol, FechaCreacion, Activo)
VALUES ('familia@centro.local', '$2a$11$MSFHhJuQWAooQDCzfZKoNuCKgY/B/lr40SF8ZDsa9I72gW.ic.VJm', 'Padre', 'Familia', 'Padre', NOW(), 1);



INSERT INTO Familias (TelefonoContacto, ResponsablePrincipalNombre, ResponsablePrincipalApellido, ResponsablePrincipalDNI, ResponsablePrincipalDireccion, ResponsablePrincipalEmail, ResponsablePrincipalTelefono, ResponsablePrincipalRelacion, Responsable2Nombre, Responsable2Apellido, Responsable2DNI, Responsable2Direccion, Responsable2Telefono, Responsable2Relacion, FechaCreacion)
VALUES ('987654321', 'María', 'Gómez', '12345678', 'Av. Siempre Viva 123', 'familia@centro.local', '987654321', 'Madre', 'Carlos', 'Gómez', '87654321', 'Av. Siempre Viva 123', '987654322', 'Padre', NOW());
SET @familia1 = LAST_INSERT_ID();

INSERT INTO Familias (TelefonoContacto, ResponsablePrincipalNombre, ResponsablePrincipalApellido, ResponsablePrincipalDNI, ResponsablePrincipalDireccion, ResponsablePrincipalEmail, ResponsablePrincipalTelefono, ResponsablePrincipalRelacion, Responsable2Nombre, Responsable2Apellido, Responsable2DNI, Responsable2Direccion, Responsable2Telefono, Responsable2Relacion, FechaCreacion)
VALUES ('912345000', 'Lucía', 'Fernández', '23456789', 'Calle Luna 45', 'lucia.fernandez@email.com', '912345000', 'Madre', 'Sofía', 'Fernández', '33445566', 'Calle Luna 45', '912345001', 'Tía', NOW());
SET @familia2 = LAST_INSERT_ID();

INSERT INTO Familias (TelefonoContacto, ResponsablePrincipalNombre, ResponsablePrincipalApellido, ResponsablePrincipalDNI, ResponsablePrincipalDireccion, ResponsablePrincipalEmail, ResponsablePrincipalTelefono, ResponsablePrincipalRelacion, Responsable2Nombre, Responsable2Apellido, Responsable2DNI, Responsable2Direccion, Responsable2Telefono, Responsable2Relacion, FechaCreacion)
VALUES ('911223344', 'José', 'Martínez', '34567890', 'Pje. Sol 8', 'jose.martinez@email.com', '911223344', 'Padre', 'Ana', 'Martínez', '44556677', 'Pje. Sol 8', '911223345', 'Madre', NOW());
SET @familia3 = LAST_INSERT_ID();

INSERT INTO Familias (TelefonoContacto, ResponsablePrincipalNombre, ResponsablePrincipalApellido, ResponsablePrincipalDNI, ResponsablePrincipalDireccion, ResponsablePrincipalEmail, ResponsablePrincipalTelefono, ResponsablePrincipalRelacion, Responsable2Nombre, Responsable2Apellido, Responsable2DNI, Responsable2Direccion, Responsable2Telefono, Responsable2Relacion, FechaCreacion)
VALUES ('900111222', 'Pedro', 'Ramírez', '55667788', 'Av. Los Pinos 50', 'pedro.ramirez@email.com', '900111222', 'Padre', 'Laura', 'Ramírez', '66778899', 'Av. Los Pinos 50', '900111223', 'Madre', NOW());
SET @familia4 = LAST_INSERT_ID();

INSERT INTO Familias (TelefonoContacto, ResponsablePrincipalNombre, ResponsablePrincipalApellido, ResponsablePrincipalDNI, ResponsablePrincipalDireccion, ResponsablePrincipalEmail, ResponsablePrincipalTelefono, ResponsablePrincipalRelacion, Responsable2Nombre, Responsable2Apellido, Responsable2DNI, Responsable2Direccion, Responsable2Telefono, Responsable2Relacion, FechaCreacion)
VALUES ('933222333', 'Miguel', 'Torres', '77889900', 'Calle Robles 77', 'miguel.torres@email.com', '933222333', 'Padre', 'Paula', 'Torres', '88990011', 'Calle Robles 77', '933222334', 'Madre', NOW());
SET @familia5 = LAST_INSERT_ID();


-- Familia 1: Gómez
INSERT INTO Pacientes (Nombres, Apellidos, FechaNacimiento, Sexo, FechaCreacion, NombreContactoEmergencia, NumeroContactoEmergencia, FamiliaId, DNI)
VALUES ('Lucas', 'Gómez', '2018-05-15', 0, NOW(), 'María Gómez', '987654321', @familia1, '11111111');
SET @paciente1 = LAST_INSERT_ID();
INSERT INTO Pacientes (Nombres, Apellidos, FechaNacimiento, Sexo, FechaCreacion, NombreContactoEmergencia, NumeroContactoEmergencia, FamiliaId, DNI)
VALUES ('Camila', 'Gómez', '2020-09-10', 1, NOW(), 'Carlos Gómez', '987654322', @familia1, '11111112');
SET @paciente2 = LAST_INSERT_ID();

-- Familia 2: Fernández
INSERT INTO Pacientes (Nombres, Apellidos, FechaNacimiento, Sexo, FechaCreacion, NombreContactoEmergencia, NumeroContactoEmergencia, FamiliaId, DNI)
VALUES ('Sofía', 'Fernández', '2016-03-20', 1, NOW(), 'Lucía Fernández', '912345000', @familia2, '22222222');
SET @paciente3 = LAST_INSERT_ID();
INSERT INTO Pacientes (Nombres, Apellidos, FechaNacimiento, Sexo, FechaCreacion, NombreContactoEmergencia, NumeroContactoEmergencia, FamiliaId, DNI)
VALUES ('Valentina', 'Fernández', '2019-07-01', 1, NOW(), 'Sofía Fernández', '912345001', @familia2, '22222223');
SET @paciente4 = LAST_INSERT_ID();

-- Familia 3: Martínez
INSERT INTO Pacientes (Nombres, Apellidos, FechaNacimiento, Sexo, FechaCreacion, NombreContactoEmergencia, NumeroContactoEmergencia, FamiliaId, DNI)
VALUES ('Mateo', 'Martínez', '2019-11-10', 0, NOW(), 'José Martínez', '911223344', @familia3, '33333333');
SET @paciente5 = LAST_INSERT_ID();
INSERT INTO Pacientes (Nombres, Apellidos, FechaNacimiento, Sexo, FechaCreacion, NombreContactoEmergencia, NumeroContactoEmergencia, FamiliaId, DNI)
VALUES ('Diego', 'Martínez', '2017-12-12', 0, NOW(), 'Ana Martínez', '911223345', @familia3, '33333334');
SET @paciente6 = LAST_INSERT_ID();

-- Familia 4: Ramírez
INSERT INTO Pacientes (Nombres, Apellidos, FechaNacimiento, Sexo, FechaCreacion, NombreContactoEmergencia, NumeroContactoEmergencia, FamiliaId, DNI)
VALUES ('Luciana', 'Ramírez', '2015-08-22', 1, NOW(), 'Laura Ramírez', '900111223', @familia4, '44444444');
SET @paciente7 = LAST_INSERT_ID();
INSERT INTO Pacientes (Nombres, Apellidos, FechaNacimiento, Sexo, FechaCreacion, NombreContactoEmergencia, NumeroContactoEmergencia, FamiliaId, DNI)
VALUES ('Gabriel', 'Ramírez', '2013-04-17', 0, NOW(), 'Pedro Ramírez', '900111222', @familia4, '44444445');
SET @paciente8 = LAST_INSERT_ID();

-- Familia 5: Torres
INSERT INTO Pacientes (Nombres, Apellidos, FechaNacimiento, Sexo, FechaCreacion, NombreContactoEmergencia, NumeroContactoEmergencia, FamiliaId, DNI)
VALUES ('Martina', 'Torres', '2017-10-05', 1, NOW(), 'Paula Torres', '933222334', @familia5, '55555555');
SET @paciente9 = LAST_INSERT_ID();
INSERT INTO Pacientes (Nombres, Apellidos, FechaNacimiento, Sexo, FechaCreacion, NombreContactoEmergencia, NumeroContactoEmergencia, FamiliaId, DNI)
VALUES ('Emilio', 'Torres', '2014-02-28', 0, NOW(), 'Miguel Torres', '933222333', @familia5, '55555556');
SET @paciente10 = LAST_INSERT_ID();

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

INSERT INTO Especialidades (Nombre, Descripcion, FechaCreacion)
VALUES ('Terapia Ocupacional', 'Mejora de habilidades motoras y funcionales', NOW());
SET @esp4Id = LAST_INSERT_ID();

-- Se incluyen ahora DNI y Correo (ambos opcionales pero rellenados para consistencia)
INSERT INTO Terapeutas (Nombres, Apellidos, DNI, Correo, EspecialidadId, Presentacion, Telefono, Direccion, Activo, FechaCreacion)
VALUES ('Ana', 'López', '46881234', 'terapeuta@centro.local', @esp1Id, 'Fonoaudióloga con 8 años de experiencia', '912345678', 'Calle Falsa 45', 1, NOW());
SET @terapeuta1Id = LAST_INSERT_ID();

INSERT INTO Terapeutas (Nombres, Apellidos, DNI, Correo, EspecialidadId, Presentacion, Telefono, Direccion, Activo, FechaCreacion)
VALUES ('Raúl', 'Pérez', '42119876', 'raul.perez@centro.local', @esp3Id, 'Fisioterapeuta pediátrico', '912345679', 'Calle Real 12', 1, NOW());
SET @terapeuta2Id = LAST_INSERT_ID();

INSERT INTO Terapeutas (Nombres, Apellidos, DNI, Correo, EspecialidadId, Presentacion, Telefono, Direccion, Activo, FechaCreacion)
VALUES ('Marcos', 'García', '47881222', 'marcos.garcia@centro.local', @esp4Id, 'Terapeuta ocupacional', '912345680', 'Av. Central 7', 1, NOW());
SET @terapeuta3Id = LAST_INSERT_ID();

INSERT INTO Terapeutas (Nombres, Apellidos, DNI, Correo, EspecialidadId, Presentacion, Telefono, Direccion, Activo, FechaCreacion)
VALUES ('Elena', 'Suarez', '41223344', 'elena.suarez@centro.local', @esp2Id, 'Psicóloga infantil', '912345681', 'Pza. Mayor 2', 1, NOW());
SET @terapeuta4Id = LAST_INSERT_ID();

-- 6) Tipos de sesión (sesiones)
-- Each TipoSesion is named after its Especialidad and all last 45 minutes
INSERT INTO TiposSesion (Nombre, DuracionMinutos, Precio, Descripcion, EspecialidadId)
VALUES ('Sesión de Terapia del Lenguaje', 45, 30.00, 'Sesión relacionada con la especialidad Terapia del Lenguaje', @esp1Id);
SET @tipo1Id = LAST_INSERT_ID();

INSERT INTO TiposSesion (Nombre, DuracionMinutos, Precio, Descripcion, EspecialidadId)
VALUES ('Sesión de Psicología Infantil', 45, 35.00, 'Sesión relacionada con la especialidad Psicología Infantil', @esp2Id);
SET @tipo2Id = LAST_INSERT_ID();

INSERT INTO TiposSesion (Nombre, DuracionMinutos, Precio, Descripcion, EspecialidadId)
VALUES ('Sesión de Fisioterapia Pediátrica', 45, 40.00, 'Sesión relacionada con la especialidad Fisioterapia Pediátrica', @esp3Id);
SET @tipo3Id = LAST_INSERT_ID();

INSERT INTO TiposSesion (Nombre, DuracionMinutos, Precio, Descripcion, EspecialidadId)
VALUES ('Sesión de Terapia Ocupacional', 45, 25.00, 'Sesión relacionada con la especialidad Terapia Ocupacional', @esp4Id);
SET @tipo4Id = LAST_INSERT_ID();

-- 7) Franjas de disponibilidad (recurrentes por día de semana)

-- Franjas de disponibilidad actualizadas
-- Ana: lunes a viernes de 09:00:00 a 19:00:00
INSERT INTO FranjasDisponibilidad (TerapeutaId, Fecha, DiaSemana, HoraInicio, HoraFin, Recurrente) VALUES
(@terapeuta1Id, NULL, 1, '09:00:00', '19:00:00', 1), -- lunes
(@terapeuta1Id, NULL, 2, '09:00:00', '19:00:00', 1), -- martes
(@terapeuta1Id, NULL, 3, '09:00:00', '19:00:00', 1), -- miércoles
(@terapeuta1Id, NULL, 4, '09:00:00', '19:00:00', 1), -- jueves
(@terapeuta1Id, NULL, 5, '09:00:00', '19:00:00', 1), -- viernes
(@terapeuta1Id, NULL, 6, '07:00:00', '19:00:00', 1); -- sabado

-- Raúl: lunes, miércoles y viernes de 07:00:00 a 15:00:00
INSERT INTO FranjasDisponibilidad (TerapeutaId, Fecha, DiaSemana, HoraInicio, HoraFin, Recurrente) VALUES
(@terapeuta2Id, NULL, 1, '07:00:00', '15:00:00', 1), -- lunes
(@terapeuta2Id, NULL, 3, '07:00:00', '15:00:00', 1), -- miércoles
(@terapeuta2Id, NULL, 5, '07:00:00', '15:00:00', 1); -- viernes

-- Marcos: martes y jueves de 08:00:00 a 19:00:00
INSERT INTO FranjasDisponibilidad (TerapeutaId, Fecha, DiaSemana, HoraInicio, HoraFin, Recurrente) VALUES
(@terapeuta3Id, NULL, 2, '08:00:00', '19:00:00', 1), -- martes
(@terapeuta3Id, NULL, 4, '08:00:00', '19:00:00', 1); -- jueves

-- Elena: jueves, viernes y sábado de 07:00:00 a 14:00:00
INSERT INTO FranjasDisponibilidad (TerapeutaId, Fecha, DiaSemana, HoraInicio, HoraFin, Recurrente) VALUES
(@terapeuta4Id, NULL, 4, '07:00:00', '14:00:00', 1), -- jueves
(@terapeuta4Id, NULL, 5, '07:00:00', '14:00:00', 1), -- viernes
(@terapeuta4Id, NULL, 6, '07:00:00', '14:00:00', 1); -- sábado



-- 8) Citas (usar `TipoSesionId` acorde a entidades actuales)

INSERT INTO Citas (Fecha, Motivo, Estado, Notas, FechaCreacion, PacienteId, TerapeutaId, TipoSesionId, DuracionMinutos) VALUES

('2025-12-02 09:00:00', 'Control mensual', 'Completada', 'Sesión completada con éxito', NOW(), @paciente1, @terapeuta1Id, @tipo1Id, 45), -- Lucas Gómez con Ana López
('2025-12-05 10:00:00', 'Evaluación', 'Completada', 'Evaluación finalizada, sin novedades', NOW(), @paciente2, @terapeuta1Id, @tipo1Id, 45),
('2025-12-10 11:00:00', 'Revisión', 'NoAsistio', 'Paciente no asistió a la cita', NOW(), @paciente1, @terapeuta1Id, @tipo1Id, 45), -- Lucas Gómez con Ana López
('2025-12-12 12:00:00', 'Seguimiento', 'NoAsistio', 'No se presentó el paciente', NOW(), @paciente3, @terapeuta2Id, @tipo3Id, 45),
('2025-12-15 13:00:00', 'Consulta', 'Cancelada', 'Cita cancelada por el paciente', NOW(), @paciente4, @terapeuta2Id, @tipo3Id, 45),
('2025-12-20 14:00:00', 'Terapia especial', 'Cancelada', 'Cancelada por motivos personales', NOW(), @paciente5, @terapeuta3Id, @tipo4Id, 45),
('2025-12-27 16:00:00', 'Sesión de lenguaje', 'Scheduled', 'Cita especial sábado', NOW(), @paciente1, @terapeuta1Id, @tipo1Id, 45), -- Lucas Gómez con Ana (sábado)
('2025-12-29 10:00:00', 'Seguimiento', 'Scheduled', NULL, NOW(), @paciente2, @terapeuta1Id, @tipo1Id, 45), -- Camila Gómez con Ana (lunes)
('2025-12-29 07:30:00', 'Fisioterapia', 'Scheduled', NULL, NOW(), @paciente3, @terapeuta2Id, @tipo3Id, 45), -- Sofía Fernández con Raúl (lunes)
('2025-12-31 07:00:00', 'Fisioterapia', 'Scheduled', NULL, NOW(), @paciente4, @terapeuta2Id, @tipo3Id, 45), -- Valentina Fernández con Raúl (miércoles)
('2026-01-02 09:00:00', 'Terapia ocupacional', 'Scheduled', NULL, NOW(), @paciente5, @terapeuta3Id, @tipo4Id, 45), -- Mateo Martínez con Marcos (viernes)
('2026-01-06 08:00:00', 'Terapia ocupacional', 'Scheduled', NULL, NOW(), @paciente6, @terapeuta3Id, @tipo4Id, 45), -- Diego Martínez con Marcos (martes)
('2026-01-08 07:00:00', 'Psicología infantil', 'Scheduled', NULL, NOW(), @paciente7, @terapeuta4Id, @tipo2Id, 45), -- Luciana Ramírez con Elena (jueves)
('2026-01-09 07:00:00', 'Psicología infantil', 'Scheduled', NULL, NOW(), @paciente8, @terapeuta4Id, @tipo2Id, 45), -- Gabriel Ramírez con Elena (viernes)
('2026-01-03 09:00:00', 'Lenguaje', 'Scheduled', NULL, NOW(), @paciente9, @terapeuta1Id, @tipo1Id, 45), -- Martina Torres con Ana (sábado)
('2025-12-27 09:00:00', 'Control especialidad', 'Scheduled', NULL, NOW(), @paciente2, @terapeuta1Id, @tipo1Id, 45),
('2025-12-30 09:00:00', 'Seguimiento especialidad', 'Scheduled', NULL, NOW(), @paciente2, @terapeuta1Id, @tipo1Id, 45),
('2025-12-27 10:00:00', 'Control especialidad', 'Scheduled', NULL, NOW(), @paciente1, @terapeuta1Id, @tipo1Id, 45),
('2025-12-30 10:00:00', 'Seguimiento especialidad', 'Scheduled', NULL, NOW(), @paciente1, @terapeuta1Id, @tipo1Id, 45),
('2026-01-05 09:00:00', 'Lenguaje', 'Scheduled', NULL, NOW(), @paciente10, @terapeuta1Id, @tipo1Id, 45); -- Emilio Torres con Ana (lunes)
SET @cita1Id = LAST_INSERT_ID();

-- 9) Notas de sesión (asociadas a citas)
INSERT INTO NotaSesion (CitaId, TerapeutaId, Notas, FechaCreacion)
VALUES
(@cita1Id, @terapeuta1Id, 'Evaluación: articulación y comprensión dentro de límites esperados para la edad, recomienda 2 sesiones/semana.', NOW());

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
