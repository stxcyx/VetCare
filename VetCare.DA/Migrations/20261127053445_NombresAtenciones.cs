using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetCare.DA.Migrations
{
    public partial class NombresAtenciones : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE
                    dbo.SP_Atencion_ListarPendientes
                AS
                BEGIN
                    SET NOCOUNT ON;

                    SELECT
                        C.*,
                        M.Nombre AS NombreMascota,
                        V.Nombre AS NombreVeterinario
                    FROM dbo.Citas C
                    INNER JOIN dbo.Mascotas M
                        ON C.MascotaId = M.MascotaId
                    INNER JOIN dbo.Veterinarios V
                        ON C.VeterinarioId = V.VeterinarioId
                    WHERE C.Estado = 'Programada'
                    ORDER BY C.FechaHora;
                END;
            ");

            migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE
                    dbo.SP_Atencion_ListarHistorial
                AS
                BEGIN
                    SET NOCOUNT ON;

                    SELECT
                        C.*,
                        M.Nombre AS NombreMascota,
                        V.Nombre AS NombreVeterinario
                    FROM dbo.Citas C
                    INNER JOIN dbo.Mascotas M
                        ON C.MascotaId = M.MascotaId
                    INNER JOIN dbo.Veterinarios V
                        ON C.VeterinarioId = V.VeterinarioId
                    WHERE C.Estado = 'Atendida'
                    ORDER BY C.FechaHora DESC;
                END;
            ");

            migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE
                    dbo.SP_Atencion_Obtener
                    @CitaId INT
                AS
                BEGIN
                    SET NOCOUNT ON;

                    SELECT
                        C.*,
                        M.Nombre AS NombreMascota,
                        V.Nombre AS NombreVeterinario
                    FROM dbo.Citas C
                    INNER JOIN dbo.Mascotas M
                        ON C.MascotaId = M.MascotaId
                    INNER JOIN dbo.Veterinarios V
                        ON C.VeterinarioId = V.VeterinarioId
                    WHERE C.CitaId = @CitaId;
                END;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE
                    dbo.SP_Atencion_ListarPendientes
                AS
                BEGIN
                    SET NOCOUNT ON;

                    SELECT *
                    FROM dbo.Citas
                    WHERE Estado = 'Programada'
                    ORDER BY FechaHora;
                END;
            ");

            migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE
                    dbo.SP_Atencion_ListarHistorial
                AS
                BEGIN
                    SET NOCOUNT ON;

                    SELECT *
                    FROM dbo.Citas
                    WHERE Estado = 'Atendida'
                    ORDER BY FechaHora DESC;
                END;
            ");

            migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE
                    dbo.SP_Atencion_Obtener
                    @CitaId INT
                AS
                BEGIN
                    SET NOCOUNT ON;

                    SELECT *
                    FROM dbo.Citas
                    WHERE CitaId = @CitaId;
                END;
            ");
        }
    }
}