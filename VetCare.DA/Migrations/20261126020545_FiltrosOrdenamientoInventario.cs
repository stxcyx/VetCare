using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetCare.DA.Migrations
{
    public partial class FiltrosOrdenamientoInventario : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                EXEC(N'
                    ALTER PROCEDURE dbo.SP_Producto_Listar
                        @Buscar NVARCHAR(100) = NULL,
                        @Pagina INT = 1,
                        @Tamano INT = 10,
                        @Categoria NVARCHAR(20) = NULL,
                        @Estado NVARCHAR(20) = NULL,
                        @OrdenarPor NVARCHAR(20) = ''Nombre'',
                        @Direccion NVARCHAR(4) = ''ASC''
                    AS
                    BEGIN
                        SET NOCOUNT ON;

                        SELECT p.*, pr.Nombre AS NombreProveedor
                        FROM dbo.Productos p
                        INNER JOIN dbo.Proveedores pr
                            ON p.ProveedorId = pr.ProveedorId
                        WHERE
                            (
                                @Buscar IS NULL
                                OR p.Nombre LIKE ''%'' + @Buscar + ''%''
                                OR p.Categoria LIKE ''%'' + @Buscar + ''%''
                            )
                            AND
                            (
                                @Categoria IS NULL
                                OR p.Categoria = @Categoria
                            )
                            AND
                            (
                                @Estado IS NULL
                                OR (@Estado = ''Disponible''
                                    AND p.Activo = 1
                                    AND p.Stock > p.StockMinimo)
                                OR (@Estado = ''StockBajo''
                                    AND p.Activo = 1
                                    AND p.Stock <= p.StockMinimo)
                                OR (@Estado = ''Inactivo''
                                    AND p.Activo = 0)
                            )
                        ORDER BY
                            CASE
                                WHEN @OrdenarPor = ''Nombre''
                                AND @Direccion = ''ASC''
                                THEN p.Nombre
                            END ASC,

                            CASE
                                WHEN @OrdenarPor = ''Nombre''
                                AND @Direccion = ''DESC''
                                THEN p.Nombre
                            END DESC,

                            CASE
                                WHEN @OrdenarPor = ''Categoria''
                                AND @Direccion = ''ASC''
                                THEN p.Categoria
                            END ASC,

                            CASE
                                WHEN @OrdenarPor = ''Categoria''
                                AND @Direccion = ''DESC''
                                THEN p.Categoria
                            END DESC,

                            CASE
                                WHEN @OrdenarPor = ''Stock''
                                AND @Direccion = ''ASC''
                                THEN p.Stock
                            END ASC,

                            CASE
                                WHEN @OrdenarPor = ''Stock''
                                AND @Direccion = ''DESC''
                                THEN p.Stock
                            END DESC,

                            p.Nombre,
                            p.ProductoId

                        OFFSET (@Pagina - 1) * @Tamano ROWS
                        FETCH NEXT @Tamano ROWS ONLY;

                        SELECT COUNT(*) AS TotalRegistros
                        FROM dbo.Productos p
                        INNER JOIN dbo.Proveedores pr
                            ON p.ProveedorId = pr.ProveedorId
                        WHERE
                            (
                                @Buscar IS NULL
                                OR p.Nombre LIKE ''%'' + @Buscar + ''%''
                                OR p.Categoria LIKE ''%'' + @Buscar + ''%''
                            )
                            AND
                            (
                                @Categoria IS NULL
                                OR p.Categoria = @Categoria
                            )
                            AND
                            (
                                @Estado IS NULL
                                OR (@Estado = ''Disponible''
                                    AND p.Activo = 1
                                    AND p.Stock > p.StockMinimo)
                                OR (@Estado = ''StockBajo''
                                    AND p.Activo = 1
                                    AND p.Stock <= p.StockMinimo)
                                OR (@Estado = ''Inactivo''
                                    AND p.Activo = 0)
                            );
                    END
                ');
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                EXEC(N'
                    ALTER PROCEDURE dbo.SP_Producto_Listar
                        @Buscar NVARCHAR(100) = NULL,
                        @Pagina INT = 1,
                        @Tamano INT = 10
                    AS
                    BEGIN
                        SET NOCOUNT ON;

                        SELECT p.*, pr.Nombre AS NombreProveedor
                        FROM Productos p
                        INNER JOIN Proveedores pr
                            ON p.ProveedorId = pr.ProveedorId
                        WHERE @Buscar IS NULL
                            OR p.Nombre LIKE ''%'' + @Buscar + ''%''
                            OR p.Categoria LIKE ''%'' + @Buscar + ''%''
                        ORDER BY p.Nombre, p.ProductoId
                        OFFSET (@Pagina - 1) * @Tamano ROWS
                        FETCH NEXT @Tamano ROWS ONLY;

                        SELECT COUNT(*) AS TotalRegistros
                        FROM Productos p
                        WHERE @Buscar IS NULL
                            OR p.Nombre LIKE ''%'' + @Buscar + ''%''
                            OR p.Categoria LIKE ''%'' + @Buscar + ''%'';
                    END
                ');
            ");
        }
    }
}