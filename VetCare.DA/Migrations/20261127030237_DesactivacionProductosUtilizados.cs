using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetCare.DA.Migrations
{
    public partial class DesactivacionProductosUtilizados : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                EXEC(N'
                    CREATE OR ALTER PROCEDURE dbo.SP_Producto_Eliminar
                        @ProductoId INT
                    AS
                    BEGIN
                        SET NOCOUNT ON;
                        SET XACT_ABORT ON;

                        BEGIN TRY

                            BEGIN TRANSACTION;

                            IF NOT EXISTS
                            (
                                SELECT 1
                                FROM dbo.Productos WITH (XLOCK, HOLDLOCK)
                                WHERE ProductoId = @ProductoId
                            )
                            BEGIN
                                COMMIT TRANSACTION;

                                SELECT CAST(0 AS BIT);

                                RETURN;
                            END;

                            IF EXISTS
                            (
                                SELECT 1
                                FROM dbo.CitaProductos WITH (UPDLOCK, HOLDLOCK)
                                WHERE ProductoId = @ProductoId
                            )
                            BEGIN

                                UPDATE dbo.Productos
                                SET Activo = 0
                                WHERE ProductoId = @ProductoId;

                            END
                            ELSE
                            BEGIN

                                DELETE FROM dbo.Productos
                                WHERE ProductoId = @ProductoId;

                            END;

                            COMMIT TRANSACTION;

                            SELECT CAST(1 AS BIT);

                        END TRY
                        BEGIN CATCH

                            IF @@TRANCOUNT > 0
                                ROLLBACK TRANSACTION;

                            THROW;

                        END CATCH;
                    END
                ');
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                EXEC(N'
                    CREATE OR ALTER PROCEDURE dbo.SP_Producto_Eliminar
                        @ProductoId INT
                    AS
                    BEGIN
                        SET NOCOUNT ON;

                        IF EXISTS
                        (
                            SELECT 1
                            FROM dbo.CitaProductos
                            WHERE ProductoId = @ProductoId
                        )
                        BEGIN
                            SELECT CAST(0 AS BIT);
                            RETURN;
                        END;

                        DELETE FROM dbo.Productos
                        WHERE ProductoId = @ProductoId;

                        SELECT CAST(
                            CASE
                                WHEN @@ROWCOUNT > 0 THEN 1
                                ELSE 0
                            END AS BIT
                        );
                    END
                ');
            ");
        }
    }
}