
VetCare

Sistema web para la gestión de una clínica veterinaria: clientes, mascotas, veterinarios, citas, atención clínica, inventario, dashboard y reportes, con acceso por roles (Administrador, Recepcionista y Veterinario).

Proyecto del Grupo 6.

Arquitectura
La solución sigue Clean Architecture y los principios SOLID. Cada capa es un proyecto:
VetCare.Abstracciones (biblioteca de clases): modelos e interfaces.
VetCare.DA (biblioteca de clases): acceso a datos con Entity Framework Core, migraciones y datos semilla.
VetCare.Reglas (biblioteca de clases): reglas de negocio (RN-01 a RN-06).
VetCare.Flujo (biblioteca de clases): coordina cada operación entre Reglas y DA.
VetCare.API (ASP.NET Core Web API): endpoints REST, documentados con Swagger.
VetCare.Web (ASP.NET Core MVC): interfaz de usuario; consume la API por HTTP.

Recorrido de una petición: Web (MVC) → Proxy (HttpClient) → API → Flujo → Reglas → DA → SQL Server.
La Web solo usa la base de datos para el inicio de sesión (ASP.NET Core Identity). Todos los datos de los módulos llegan por la API.

Requisitos
SDK de .NET 10.
SQL Server LocalDB (se instala con Visual Studio).
Herramienta de Entity Framework: dotnet tool install --global dotnet-ef

Cómo ejecutarlo
1. Clonar el repositorio:
   git clone https://github.com/stxcyx/VetCare.git
   cd VetCare
   
2. Guardar la clave de los usuarios de prueba como secreto de la API:
   dotnet user-secrets set "Seed:Password" "<clave de prueba>" --project VetCare.API
   
3. Ejecutar la API y la Web.
   - **Visual Studio:** abrir `VetCare.slnx`. El perfil de inicio ya arranca los dos proyectos, pulsar F5.
   - **Terminal:** en dos terminales distintas:
     dotnet run --project VetCare.API --launch-profile https
     dotnet run --project VetCare.Web --launch-profile https
     

La API crea la base de datos `VetCare`, aplica las migraciones y carga los datos de prueba al arrancar. No hace falta `dotnet ef database update`.

Direcciones:
Web: https://localhost:7060
API (Swagger): https://localhost:7100/swagger

Usuarios de prueba
Todos usan la clave configurada en el paso 2.
admin@vetcare.test: Administrador.
recepcion@vetcare.test: Recepcionista.
lmora@vetcare.test: Veterinario (Dra. Laura Mora).
jperez@vetcare.test: Veterinario (Dr. José Pérez).

Endpoints principales de la API
GET y POST /api/clientes: listar y registrar clientes.
GET y POST /api/mascotas: listar y registrar mascotas.
GET y POST /api/veterinarios: listar y registrar veterinarios.
GET /api/veterinarios/{id}/disponibilidad?fecha=aaaa-mm-dd: horarios libres de un veterinario (HU-26).
GET y POST /api/citas: agenda y nuevas citas.
GET /api/dashboard/resumen: indicadores del dashboard.
GET /api/reportes/exportar-csv: reporte de citas en CSV.
La lista completa, con ejemplos, está en Swagger.

Estructura de carpetas
VetCare.slnx
├── VetCare.Abstracciones/   Modelos/, Interfaces/, Comunes/
├── VetCare.DA/              ApplicationDbContext, SeedData, Repositorios/, Migrations/
├── VetCare.Reglas/          Una clase de reglas por módulo
├── VetCare.Flujo/           Una clase de flujo por módulo
├── VetCare.API/             Controllers/ (REST)
└── VetCare.Web/             Controllers/, Proxies/, Views/, wwwroot/

Convención de ramas
- Una rama por historia de usuario: `feature/HU-xx-nombre`.
- Los cambios entran a `main` solo por pull request.
- Antes de crear una migración: `Fetch` y `Pull` de `main`, para no chocar en `ApplicationDbContextModelSnapshot.cs`.

Problemas comunes
Falta el secreto Seed:Password: repetir el paso 2.
No conecta a la base: ejecutar sqllocaldb start MSSQLLocalDB.
Error de base de datos tras actualizar: ejecutar dotnet ef database drop --project VetCare.DA --startup-project VetCare.API --force y volver a arrancar.
El navegador avisa que el certificado no es de confianza: ejecutar dotnet dev-certs https --trust.
Al compilar sale «The process cannot access the file … .dll»: detener la aplicación antes de compilar.
