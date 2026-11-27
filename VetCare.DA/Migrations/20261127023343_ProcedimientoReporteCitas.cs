using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetCare.DA.Migrations
{
    public partial class ProcedimientoReporteCitas : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                EXEC(N'
                    CREATE OR ALTER PROCEDURE dbo.SP_Reporte_Citas

                        @FechaInicio DATE = NULL,
                        @FechaFin DATE = NULL,
                        @VeterinarioId INT = NULL

                    AS
                    BEGIN
                        SET NOCOUNT ON;

                        SELECT
                            C.CitaId,
                            C.FechaHora,

                            CONCAT(
                                CL.Nombre,
                                '' '',
                                CL.Apellidos
                            ) AS Cliente,

                            M.Nombre AS Mascota,

                            V.Nombre AS Veterinario,

                            C.Motivo,
                            C.Estado

                        FROM dbo.Citas C

                        INNER JOIN dbo.Mascotas M
                            ON C.MascotaId = M.MascotaId

                        INNER JOIN dbo.Clientes CL
                            ON M.ClienteId = CL.ClienteId

                        INNER JOIN dbo.Veterinarios V
                            ON C.VeterinarioId = V.VeterinarioId

                        WHERE
                            (
                                @FechaInicio IS NULL
                                OR C.FechaHora >= @FechaInicio
                            )

                            AND
                            (
                                @FechaFin IS NULL
                                OR CAST(C.FechaHora AS DATE) <= @FechaFin
                            )

                            AND
                            (
                                @VeterinarioId IS NULL
                                OR C.VeterinarioId = @VeterinarioId
                            )

                        ORDER BY
                            C.FechaHora DESC,
                            C.CitaId DESC;
                    END
                ');
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP PROCEDURE IF EXISTS dbo.SP_Reporte_Citas;
            ");
        }
    }
}