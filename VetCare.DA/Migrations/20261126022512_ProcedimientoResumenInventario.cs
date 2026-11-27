using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetCare.DA.Migrations
{
    public partial class ProcedimientoResumenInventario : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                EXEC(N'
                    CREATE PROCEDURE dbo.SP_Producto_Resumen
                    AS
                    BEGIN
                        SET NOCOUNT ON;

                        SELECT
                            COUNT(CASE
                                WHEN Activo = 1
                                THEN 1
                            END) AS ProductosActivos,

                            COUNT(CASE
                                WHEN Activo = 1
                                AND Stock <= StockMinimo
                                THEN 1
                            END) AS StockBajo,

                            COUNT(CASE
                                WHEN Activo = 1
                                AND Categoria = ''Vacuna''
                                THEN 1
                            END) AS Vacunas,

                            COUNT(CASE
                                WHEN Activo = 1
                                AND Categoria = ''Medicamento''
                                THEN 1
                            END) AS Medicamentos

                        FROM dbo.Productos;
                    END
                ');
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP PROCEDURE IF EXISTS dbo.SP_Producto_Resumen;
            ");
        }
    }
}