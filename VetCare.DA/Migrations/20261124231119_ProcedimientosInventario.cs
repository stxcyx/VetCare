using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetCare.DA.Migrations
{
    /// <inheritdoc />
    public partial class ProcedimientosInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Listar productos
            migrationBuilder.Sql(@"
                CREATE PROCEDURE SP_Producto_Listar
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
                        OR p.Nombre LIKE '%' + @Buscar + '%'
                        OR p.Categoria LIKE '%' + @Buscar + '%'
                    ORDER BY p.Nombre, p.ProductoId
                    OFFSET (@Pagina - 1) * @Tamano ROWS
                    FETCH NEXT @Tamano ROWS ONLY;

                    SELECT COUNT(*) AS TotalRegistros
                    FROM Productos p
                    WHERE @Buscar IS NULL
                        OR p.Nombre LIKE '%' + @Buscar + '%'
                        OR p.Categoria LIKE '%' + @Buscar + '%';
                END
            ");

            // Obtener producto por identificador
            migrationBuilder.Sql(@"
                CREATE PROCEDURE SP_Producto_Obtener
                    @ProductoId INT
                AS
                BEGIN
                    SET NOCOUNT ON;

                    SELECT p.*, pr.Nombre AS NombreProveedor
                    FROM Productos p
                    INNER JOIN Proveedores pr
                        ON p.ProveedorId = pr.ProveedorId
                    WHERE p.ProductoId = @ProductoId;
                END
            ");

            // Verificar si existe un producto
            migrationBuilder.Sql(@"
                CREATE PROCEDURE SP_Producto_Existe
                    @Nombre NVARCHAR(100),
                    @Categoria NVARCHAR(20),
                    @ExcluirProductoId INT = NULL
                AS
                BEGIN
                    SET NOCOUNT ON;

                    SELECT CAST(
                        CASE WHEN EXISTS (
                            SELECT 1
                            FROM Productos
                            WHERE Nombre = @Nombre
                            AND Categoria = @Categoria
                            AND (
                                @ExcluirProductoId IS NULL
                                OR ProductoId <> @ExcluirProductoId
                            )
                        )
                        THEN 1 ELSE 0 END
                    AS BIT);
                END
            ");

            // Verificar si existe un proveedor
            migrationBuilder.Sql(@"
                CREATE PROCEDURE SP_Proveedor_Existe
                    @ProveedorId INT
                AS
                BEGIN
                    SET NOCOUNT ON;

                    SELECT CAST(
                        CASE WHEN EXISTS (
                            SELECT 1
                            FROM Proveedores
                            WHERE ProveedorId = @ProveedorId
                        )
                        THEN 1 ELSE 0 END
                    AS BIT);
                END
            ");

            // Registrar producto
            migrationBuilder.Sql(@"
                CREATE PROCEDURE SP_Producto_Agregar
                    @Nombre NVARCHAR(100),
                    @Categoria NVARCHAR(20),
                    @ProveedorId INT,
                    @Precio DECIMAL(18, 2),
                    @Stock INT,
                    @StockMinimo INT,
                    @FechaVencimiento DATETIME2 = NULL,
                    @Activo BIT = 1
                AS
                BEGIN
                    SET NOCOUNT ON;

                    INSERT INTO Productos
                    (
                        Nombre,
                        Categoria,
                        ProveedorId,
                        Precio,
                        Stock,
                        StockMinimo,
                        FechaVencimiento,
                        Activo
                    )
                    VALUES
                    (
                        @Nombre,
                        @Categoria,
                        @ProveedorId,
                        @Precio,
                        @Stock,
                        @StockMinimo,
                        @FechaVencimiento,
                        @Activo
                    );

                    SELECT CAST(SCOPE_IDENTITY() AS INT);
                END
            ");

            // Actualizar producto
            migrationBuilder.Sql(@"
                CREATE PROCEDURE SP_Producto_Actualizar
                    @ProductoId INT,
                    @Nombre NVARCHAR(100),
                    @Categoria NVARCHAR(20),
                    @ProveedorId INT,
                    @Precio DECIMAL(18, 2),
                    @Stock INT,
                    @StockMinimo INT,
                    @FechaVencimiento DATETIME2 = NULL,
                    @Activo BIT
                AS
                BEGIN
                    SET NOCOUNT ON;

                    UPDATE Productos
                    SET Nombre = @Nombre,
                        Categoria = @Categoria,
                        ProveedorId = @ProveedorId,
                        Precio = @Precio,
                        Stock = @Stock,
                        StockMinimo = @StockMinimo,
                        FechaVencimiento = @FechaVencimiento,
                        Activo = @Activo
                    WHERE ProductoId = @ProductoId;

                    SELECT CAST(
                        CASE WHEN @@ROWCOUNT > 0
                        THEN 1 ELSE 0 END
                    AS BIT);
                END
            ");

            // Eliminar producto
            migrationBuilder.Sql(@"
                CREATE PROCEDURE SP_Producto_Eliminar
                    @ProductoId INT
                AS
                BEGIN
                    SET NOCOUNT ON;

                    DELETE FROM Productos
                    WHERE ProductoId = @ProductoId
                    AND NOT EXISTS (
                        SELECT 1
                        FROM CitaProductos
                        WHERE ProductoId = @ProductoId
                    );

                    SELECT CAST(
                        CASE WHEN @@ROWCOUNT > 0
                        THEN 1 ELSE 0 END
                    AS BIT);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP PROCEDURE IF EXISTS SP_Producto_Listar");

            migrationBuilder.Sql(
                "DROP PROCEDURE IF EXISTS SP_Producto_Obtener");

            migrationBuilder.Sql(
                "DROP PROCEDURE IF EXISTS SP_Producto_Existe");

            migrationBuilder.Sql(
                "DROP PROCEDURE IF EXISTS SP_Proveedor_Existe");

            migrationBuilder.Sql(
                "DROP PROCEDURE IF EXISTS SP_Producto_Agregar");

            migrationBuilder.Sql(
                "DROP PROCEDURE IF EXISTS SP_Producto_Actualizar");

            migrationBuilder.Sql(
                "DROP PROCEDURE IF EXISTS SP_Producto_Eliminar");
        }
    }
}