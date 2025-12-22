# CentroTerapia API

API backend para la gestión de un centro de terapias: usuarios, pacientes, terapeutas, familias, franjas horarias, citas y notas de sesión.

**Tecnologías:** .NET 8, ASP.NET Core Web API, Entity Framework Core, MySQL/MariaDB, JWT, Swagger, DotNetEnv.

**Estructura principal:**
- `src/CentroTerapia.API`: proyecto Web API (puertas de entrada, Swagger, autenticación).
- `src/CentroTerapia.Application`: lógica de aplicación, DTOs y servicios.
- `src/CentroTerapia.Infrastructure`: persistencia (EF Core), migrations y acceso a datos.
- `sql/inserts_seed_final.sql`: script SQL con datos semilla opcionales.

**Resumen (a rasgos generales):**

La solución expone endpoints REST para gestionar entidades del dominio (pacientes, terapeutas, citas, etc.). Usa JWT para autenticación, EF Core para persistencia y está preparada para recibir la cadena de conexión desde variables de entorno o desde `appsettings.Development.json`.

--------------------------------------------------------------------------------

Requisitos previos
- .NET SDK 8 (instalado en la máquina).
- Motor de base de datos compatible MySQL/MariaDB (por defecto el proyecto asume puerto 3306).
- (Opcional) `dotnet-ef` si desea aplicar migrations desde la CLI.

Configuración
- Clonar el repositorio:

	`git clone <repositorio> && cd <repositorio>`

```

Inicializar la base de datos

- Cambiar credenciales: modifique el archivo `.env` en la raíz del repositorio con las credenciales de su servidor. Las variables clave son `DB_HOST`, `DB_PORT`, `DB_NAME`, `DB_USER` y `DB_PASSWORD`.

- Crear la base de datos en MySQL: conecte a su servidor y cree la base de datos cuyo nombre coincida con `DB_NAME`.

DB_HOST=localhost
DB_PORT=3306
DB_NAME=CentroTerapiaDb
DB_USER=root
DB_PASSWORD=YourPassword
Jwt__Key=REPLACE_WITH_STRONG_KEY
Jwt__Issuer=CentroTerapia
Jwt__Audience=CentroTerapiaClients
```


- Ejecutar las migraciones para crear las tablas (EF Core):

	`dotnet tool install --global dotnet-ef` (si no lo tiene)

	`dotnet ef database update --project src/CentroTerapia.Infrastructure --startup-project src/CentroTerapia.API`

- Ejecutar los inserts semilla: una vez creadas las tablas, importe `sql/inserts_seed_final.sql` en la base de datos creada.

Nota: el flujo recomendado es — 1) ajustar `.env`, 2) crear la base de datos en MySQL, 3) ejecutar `dotnet ef database update` para aplicar migrations (crea tablas), 4) ejecutar el script `sql/inserts_seed_final.sql` para cargar datos iniciales.

Nota: en entorno de desarrollo, al arrancar la aplicación el `Program.cs` intentará resetear la contraseña del usuario admin (`admin@centro.local`) a `123` si existe — esto facilita pruebas locales.

Ejecutar la API
- Restaurar y compilar:

	`dotnet restore`
	`dotnet build`

- Ejecutar desde la solución (escuchará por defecto en `http://localhost:5291`):

	`dotnet run --project src/CentroTerapia.API`

- Alternativamente, entrar en la carpeta del proyecto y ejecutar:

	`cd src/CentroTerapia.API`
	`dotnet run`

Acceso y documentación
- Swagger estará disponible en: `http://localhost:5291/swagger/index.html`

Endpoints comunes (vista rápida)
- `POST /api/Auth/login` — autenticación (genera JWT).
- `GET/POST/PUT/DELETE /api/Pacientes` — gestión de pacientes.
- `GET/POST/PUT/DELETE /api/Terapeutas` — gestión de terapeutas.
- `GET/POST/PUT/DELETE /api/Citas` — gestión de citas.
- `GET/POST/PUT/DELETE /api/Especialidades`, `Familias`, `Franjas`, `NotasSesion`, `TipoSesiones` — demás recursos.

Notas adicionales
- El puerto por defecto está fijado en `5291` en `Program.cs` (se puede cambiar mediante variables de entorno o modificar el código).
- Asegúrese de usar una `Jwt:Key` fuerte en producción y de no subir secretos a Git.


Archivos importantes
- `src/CentroTerapia.API/Program.cs` — arranque y configuración del API.
- `src/CentroTerapia.API/appsettings.Development.json` — configuración local por defecto.
- `sql/inserts_seed_final.sql` — datos semilla para la base de datos.

--------------------------------------------------------------------------------

