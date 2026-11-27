using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetCare.DA.Migrations
{
    public partial class ProcedimientoListarProveedores : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                EXEC(N'
                    CREATE PROCEDURE dbo.SP_Proveedor_Listar
                    AS
                    BEGIN
                        SET NOCOUNT ON;

                        SELECT
                            ProveedorId,
                            Nombre
                        FROM dbo.Proveedores
                        ORDER BY Nombre;
                    END
                ');
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP PROCEDURE IF EXISTS dbo.SP_Proveedor_Listar;
            ");
        }
    }
}