using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetCare.DA.Migrations
{
    public partial class ProcedimientosAtencion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Listar citas pendientes de atención

            migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE dbo.SP_Atencion_ListarPendientes
                AS
                BEGIN
                    SET NOCOUNT ON;

                    SELECT *
                    FROM dbo.Citas
                    WHERE Estado = 'Programada'
                    ORDER BY FechaHora ASC;
                END;
            ");

            // Obtener una cita

            migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE dbo.SP_Atencion_Obtener
                    @CitaId INT
                AS
                BEGIN
                    SET NOCOUNT ON;

                    SELECT *
                    FROM dbo.Citas
                    WHERE CitaId = @CitaId;
                END;
            ");

            // Registrar atención y descontar inventario

            migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE dbo.SP_Atencion_Registrar
                    @CitaId INT,
                    @Diagnostico NVARCHAR(1000),
                    @Tratamiento NVARCHAR(1000),
                    @Productos NVARCHAR(MAX)
                AS
                BEGIN
                    SET NOCOUNT ON;
                    SET XACT_ABORT ON;

                    BEGIN TRY

                        BEGIN TRANSACTION;

                        -- Validar existencia de la cita

                        IF NOT EXISTS (
                            SELECT 1
                            FROM dbo.Citas WITH (UPDLOCK, HOLDLOCK)
                            WHERE CitaId = @CitaId
                        )
                        BEGIN
                            ROLLBACK TRANSACTION;

                            SELECT N'La cita no existe.' AS Mensaje;
                            RETURN;
                        END;

                        -- Validar estado de la cita

                        IF NOT EXISTS (
                            SELECT 1
                            FROM dbo.Citas
                            WHERE CitaId = @CitaId
                              AND Estado = 'Programada'
                        )
                        BEGIN
                            ROLLBACK TRANSACTION;

                            SELECT N'La cita no se encuentra programada.'
                                AS Mensaje;
                            RETURN;
                        END;

                        -- Validar diagnóstico y tratamiento

                        IF NULLIF(LTRIM(RTRIM(@Diagnostico)), '') IS NULL
                           OR NULLIF(LTRIM(RTRIM(@Tratamiento)), '') IS NULL
                        BEGIN
                            ROLLBACK TRANSACTION;

                            SELECT N'El diagnóstico y tratamiento son obligatorios.'
                                AS Mensaje;
                            RETURN;
                        END;

                        -- Validar productos recibidos

                        IF @Productos IS NULL
                           OR ISJSON(@Productos) <> 1
                           OR LEFT(LTRIM(@Productos), 1) <> '['
                        BEGIN
                            ROLLBACK TRANSACTION;

                            SELECT N'La lista de productos no es válida.'
                                AS Mensaje;
                            RETURN;
                        END;

                        DECLARE @Detalle TABLE
                        (
                            ProductoId INT,
                            Cantidad INT
                        );

                        INSERT INTO @Detalle
                        (
                            ProductoId,
                            Cantidad
                        )
                        SELECT
                            ProductoId,
                            Cantidad
                        FROM OPENJSON(@Productos)
                        WITH
                        (
                            ProductoId INT '$.ProductoId',
                            Cantidad INT '$.Cantidad'
                        );

                        -- Validar cantidades

                        IF EXISTS (
                            SELECT 1
                            FROM @Detalle
                            WHERE ProductoId IS NULL
                               OR ProductoId <= 0
                               OR Cantidad IS NULL
                               OR Cantidad <= 0
                        )
                        BEGIN
                            ROLLBACK TRANSACTION;

                            SELECT N'Los productos y cantidades deben ser válidos.'
                                AS Mensaje;
                            RETURN;
                        END;

                        -- Agrupar productos repetidos

                        DECLARE @Consumo TABLE
                        (
                            ProductoId INT PRIMARY KEY,
                            Cantidad BIGINT
                        );

                        INSERT INTO @Consumo
                        (
                            ProductoId,
                            Cantidad
                        )
                        SELECT
                            ProductoId,
                            SUM(CAST(Cantidad AS BIGINT))
                        FROM @Detalle
                        GROUP BY ProductoId;

                        -- Validar productos activos y existentes

                        IF EXISTS (
                            SELECT 1
                            FROM @Consumo C
                            LEFT JOIN dbo.Productos P
                                WITH (UPDLOCK, HOLDLOCK)
                                ON P.ProductoId = C.ProductoId
                            WHERE P.ProductoId IS NULL
                               OR P.Activo = 0
                        )
                        BEGIN
                            ROLLBACK TRANSACTION;

                            SELECT N'Uno de los productos no existe o está inactivo.'
                                AS Mensaje;
                            RETURN;
                        END;

                        -- Validar existencias

                        DECLARE @ProductoSinStock NVARCHAR(100);

                        SELECT TOP 1
                            @ProductoSinStock = P.Nombre
                        FROM dbo.Productos P
                            WITH (UPDLOCK, HOLDLOCK)
                        INNER JOIN @Consumo C
                            ON P.ProductoId = C.ProductoId
                        WHERE P.Stock < C.Cantidad
                        ORDER BY P.ProductoId;

                        IF @ProductoSinStock IS NOT NULL
                        BEGIN
                            ROLLBACK TRANSACTION;

                            SELECT CONCAT(
                                N'Stock insuficiente para ',
                                @ProductoSinStock
                            ) AS Mensaje;

                            RETURN;
                        END;

                        -- Descontar productos del inventario

                        UPDATE P
                        SET P.Stock = P.Stock - C.Cantidad
                        FROM dbo.Productos P
                        INNER JOIN @Consumo C
                            ON P.ProductoId = C.ProductoId;

                        -- Registrar productos utilizados

                        INSERT INTO dbo.CitaProductos
                        (
                            CitaId,
                            ProductoId,
                            Cantidad
                        )
                        SELECT
                            @CitaId,
                            ProductoId,
                            Cantidad
                        FROM @Consumo;

                        -- Finalizar atención

                        UPDATE dbo.Citas
                        SET Diagnostico = @Diagnostico,
                            Tratamiento = @Tratamiento,
                            Estado = 'Atendida'
                        WHERE CitaId = @CitaId;

                        COMMIT TRANSACTION;

                        SELECT CAST(NULL AS NVARCHAR(500))
                            AS Mensaje;

                    END TRY
                    BEGIN CATCH

                        IF @@TRANCOUNT > 0
                            ROLLBACK TRANSACTION;

                        THROW;

                    END CATCH;
                END;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP PROCEDURE IF EXISTS
                    dbo.SP_Atencion_Registrar;

                DROP PROCEDURE IF EXISTS
                    dbo.SP_Atencion_Obtener;

                DROP PROCEDURE IF EXISTS
                    dbo.SP_Atencion_ListarPendientes;
            ");
        }
    }
}