using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetCare.DA.Migrations
{
    public partial class ProcedimientosDashboard : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                EXEC(N'
                    CREATE OR ALTER PROCEDURE dbo.SP_Dashboard_Resumen
                    AS
                    BEGIN
                        SET NOCOUNT ON;

                        DECLARE @Hoy DATE = CAST(GETDATE() AS DATE);

                        DECLARE @InicioMes DATE =
                            DATEFROMPARTS(YEAR(@Hoy), MONTH(@Hoy), 1);

                        SELECT
                            (
                                SELECT COUNT(*)
                                FROM dbo.Citas
                                WHERE FechaHora >= @Hoy
                                AND FechaHora < DATEADD(DAY, 1, @Hoy)
                            ) AS CitasHoy,

                            (
                                SELECT COUNT(*)
                                FROM dbo.Citas
                                WHERE FechaHora >= @InicioMes
                                AND FechaHora < DATEADD(MONTH, 1, @InicioMes)
                            ) AS CitasMes,

                            (
                                SELECT COUNT(*)
                                FROM dbo.Mascotas
                                WHERE Activa = 1
                            ) AS MascotasActivas,

                            (
                                SELECT COUNT(*)
                                FROM dbo.Productos
                                WHERE Activo = 1
                                AND Stock <= StockMinimo
                            ) AS ProductosStockBajo;
                    END
                ');
            ");

            migrationBuilder.Sql(@"
                EXEC(N'
                    CREATE OR ALTER PROCEDURE dbo.SP_Dashboard_CitasPorEstado
                    AS
                    BEGIN
                        SET NOCOUNT ON;

                        SELECT
                            E.Estado,
                            COUNT(C.CitaId) AS Cantidad
                        FROM
                        (
                            VALUES
                                (''Programada''),
                                (''Atendida''),
                                (''Cancelada'')
                        ) AS E(Estado)

                        LEFT JOIN dbo.Citas C
                            ON C.Estado = E.Estado

                        GROUP BY E.Estado

                        ORDER BY
                            CASE E.Estado
                                WHEN ''Programada'' THEN 1
                                WHEN ''Atendida'' THEN 2
                                WHEN ''Cancelada'' THEN 3
                            END;
                    END
                ');
            ");

            migrationBuilder.Sql(@"
                EXEC(N'
                    CREATE OR ALTER PROCEDURE dbo.SP_Dashboard_CitasPorVeterinario
                    AS
                    BEGIN
                        SET NOCOUNT ON;

                        SELECT
                            V.Nombre AS Veterinario,
                            COUNT(C.CitaId) AS Cantidad

                        FROM dbo.Veterinarios V

                        LEFT JOIN dbo.Citas C
                            ON C.VeterinarioId = V.VeterinarioId

                        GROUP BY
                            V.VeterinarioId,
                            V.Nombre

                        ORDER BY
                            Cantidad DESC,
                            V.Nombre;
                    END
                ');
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP PROCEDURE IF EXISTS dbo.SP_Dashboard_Resumen;
            ");

            migrationBuilder.Sql(@"
                DROP PROCEDURE IF EXISTS dbo.SP_Dashboard_CitasPorEstado;
            ");

            migrationBuilder.Sql(@"
                DROP PROCEDURE IF EXISTS dbo.SP_Dashboard_CitasPorVeterinario;
            ");
        }
    }
}