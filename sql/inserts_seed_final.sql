
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
VALUES ('911223344', 'José', 'Martínez', '34567890', 'Pje. Sol 8', 'jose.martinez@email.com', '911223344', 'Padre', NULL, NULL, NULL, NULL, NULL, NULL, NOW());
SET @familia3 = LAST_INSERT_ID();

INSERT INTO Pacientes (Nombres, Apellidos, FechaNacimiento, Sexo, FechaCreacion, NombreContactoEmergencia, NumeroContactoEmergencia, FamiliaId, DNI)
VALUES ('Lucas', 'Gómez', '2018-05-15', 0, NOW(), 'María Gómez', '987654321', @familia1, '11111111');
SET @paciente1 = LAST_INSERT_ID();

INSERT INTO Pacientes (Nombres, Apellidos, FechaNacimiento, Sexo, FechaCreacion, NombreContactoEmergencia, NumeroContactoEmergencia, FamiliaId, DNI)
VALUES ('Sofía', 'Fernández', '2016-03-20', 1, NOW(), 'Lucía Fernández', '912345000', @familia2, '22222222');
SET @paciente2 = LAST_INSERT_ID();

INSERT INTO Pacientes (Nombres, Apellidos, FechaNacimiento, Sexo, FechaCreacion, NombreContactoEmergencia, NumeroContactoEmergencia, FamiliaId, DNI)
VALUES ('Mateo', 'Martínez', '2019-11-10', 0, NOW(), 'José Martínez', '911223344', @familia3, '33333333');
SET @paciente3 = LAST_INSERT_ID();

INSERT INTO Pacientes (Nombres, Apellidos, FechaNacimiento, Sexo, FechaCreacion, NombreContactoEmergencia, NumeroContactoEmergencia, FamiliaId, DNI)
VALUES ('Valentina', 'Lopez', '2020-07-01', 1, NOW(), 'Ana López', '912345678', @familia2, '44444444');
SET @paciente4 = LAST_INSERT_ID();

INSERT INTO Pacientes (Nombres, Apellidos, FechaNacimiento, Sexo, FechaCreacion, NombreContactoEmergencia, NumeroContactoEmergencia, FamiliaId, DNI)
VALUES ('Diego', 'Ramos', '2017-12-12', 0, NOW(), 'Contacto N/A', '000000000', @familia3, '55555555');
SET @paciente5 = LAST_INSERT_ID();


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
INSERT INTO FranjasDisponibilidad (TerapeutaId, Fecha, DiaSemana, HoraInicio, HoraFin, Recurrente)
VALUES
(@terapeuta1Id, NULL, 1, '09:00:00', '13:00:00', 1), -- Ana, lunes
(@terapeuta1Id, NULL, 3, '14:00:00', '18:00:00', 1), -- Ana, miércoles
(@terapeuta2Id, NULL, 2, '08:30:00', '12:30:00', 1), -- Raúl, martes
(@terapeuta3Id, NULL, 4, '10:00:00', '14:00:00', 1), -- Marcos, jueves
(@terapeuta4Id, NULL, 5, '09:00:00', '13:00:00', 1); -- Elena, viernes

-- 8) Citas (usar `TipoSesionId` acorde a entidades actuales)
INSERT INTO Citas (Fecha, Motivo, Estado, Notas, FechaCreacion, PacienteId, TerapeutaId, TipoSesionId, DuracionMinutos)
VALUES
('2025-12-22 10:00:00', 'Evaluación inicial de lenguaje', 'Scheduled', 'Traer informes médicos', NOW(), @paciente1, @terapeuta1Id, @tipo1Id, 45),
('2025-12-23 09:30:00', 'Sesión de estimulación', 'Scheduled', NULL, NOW(), @paciente2, @terapeuta2Id, @tipo3Id, 45), -- usa fisio pediátrica con Raúl
('2025-12-24 11:00:00', 'Fisioterapia seguimiento', 'Scheduled', NULL, NOW(), @paciente3, @terapeuta2Id, @tipo3Id, 45),
('2025-12-25 10:30:00', 'Terapia ocupacional', 'Scheduled', 'Evaluar motricidad fina', NOW(), @paciente4, @terapeuta3Id, @tipo4Id, 45);
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
