# CentroTerapia API

API backend para la gestión de un centro de terapias: usuarios, pacientes, terapeutas, familias, franjas horarias, citas y notas de sesión.

**Tecnologías:** .NET 8, ASP.NET Core Web API, Entity Framework Core, MySQL/MariaDB, JWT, Swagger, DotNetEnv.

--------------------------------------------------------------------------------

Requisitos previos
- .NET SDK 8 (instalado en la máquina).
- Motor de base de datos compatible MySQL/MariaDB (por defecto el proyecto asume puerto 3306).
- (Opcional) `dotnet-ef` si desea aplicar migrations desde la CLI.


- Inicializar la base de datos en MysQL

- Cambiar credenciales: modifique el archivo `.env` en la raíz del repositorio con las credenciales de su servidor. Las variables clave son `DB_HOST`, `DB_PORT`, `DB_NAME`, `DB_USER` y `DB_PASSWORD`. El archivo .env debe quedar asi:

```

DB_HOST=localhost
DB_PORT=3306
DB_NAME=CentroTerapiaDb
DB_USER=root
DB_PASSWORD=YourPassword
JWT_SECRET=REPLACE_WITH_STRONG_KEY
JWT_ISSUER=CentroTerapia
JWT_AUDIENCE=CentroTerapiaClients
```
- Crear la base de datos en MySQL: conecte a su servidor y cree la base de datos con nombre CentroTerapiaDb para que coincida con `DB_NAME`.

- Ejecutar las migraciones para crear las tablas (EF Core):

	`dotnet tool install --global dotnet-ef` (si no lo tiene)

	`dotnet ef database update --project src/CentroTerapia.Infrastructure --startup-project src/CentroTerapia.API`

- Ejecutar los inserts semilla: una vez creadas las tablas, importe `sql/inserts_seed_final.sql` en la base de datos creada en MySQL.

Nota: el flujo recomendado es — 1) ajustar `.env`, 2) crear la base de datos en MySQL, 3) ejecutar `dotnet ef database update` para aplicar migrations (crea tablas), 4) ejecutar el script `sql/inserts_seed_final.sql` para cargar datos iniciales.


## Roles aceptados y credenciales de acceso

El sistema tiene 3 roles principales. Puedes ingresar con las siguientes credenciales por defecto (tras ejecutar los inserts de datos semilla):

| Rol         | Correo                   | Contraseña |
|-------------|--------------------------|------------|
| Admin       | admin@centro.local       | 123        |
| Terapeuta   | terapeuta@centro.local   | 123        |
| Padre       | familia@centro.local     | 123        |

> **Nota:** Si cambiaste los inserts o los hashes, asegúrate de actualizar estos datos según corresponda.

Ejecutar la API en consola
- Restaurar y compilar:

	`dotnet restore`
	`dotnet build`

- Ejecutar desde la solución (escuchará por defecto en `http://localhost:5192`):

	`dotnet run --project src/CentroTerapia.API`

**Resumen (a rasgos generales):**

La solución expone endpoints REST para gestionar entidades del dominio (pacientes, terapeutas, citas, etc.). Usa JWT para autenticación, EF Core para persistencia y está preparada para recibir la cadena de conexión desde variables de entorno o desde `appsettings.Development.json`.


**Estructura principal:**
- `src/CentroTerapia.API`: proyecto Web API (puertas de entrada, Swagger, autenticación).
- `src/CentroTerapia.Application`: lógica de aplicación, DTOs y servicios.
- `src/CentroTerapia.Infrastructure`: persistencia (EF Core), migrations y acceso a datos.
- `sql/inserts_seed_final.sql`: script SQL con datos semilla opcionales.

