using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetCare.DA.Migrations
{
    public partial class HistorialAtenciones : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    dbo.SP_Atencion_ListarProductos
                    @CitaId INT
                AS
                BEGIN
                    SET NOCOUNT ON;

                    SELECT
                        CP.ProductoId,
                        P.Nombre,
                        CP.Cantidad
                    FROM dbo.CitaProductos CP
                    INNER JOIN dbo.Productos P
                        ON CP.ProductoId = P.ProductoId
                    WHERE CP.CitaId = @CitaId
                    ORDER BY P.Nombre;
                END;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP PROCEDURE IF EXISTS
                    dbo.SP_Atencion_ListarHistorial;

                DROP PROCEDURE IF EXISTS
                    dbo.SP_Atencion_ListarProductos;
            ");
        }
    }
}