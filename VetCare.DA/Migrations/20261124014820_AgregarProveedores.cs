using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetCare.DA.Migrations
{
    /// <inheritdoc />
    public partial class AgregarProveedores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProveedorId",
                table: "Productos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Proveedores",
                columns: table => new
                {
                    ProveedorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedores", x => x.ProveedorId);
                });

            // Asignar proveedores a los productos existentes
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM Productos)
                BEGIN
                    INSERT INTO Proveedores (Nombre)
                    VALUES (N'Distribuidora Veterinaria');

                    DECLARE @ProveedorId INT = SCOPE_IDENTITY();

                    INSERT INTO Proveedores (Nombre)
                    VALUES (N'Distribuidora de Insumos Médicos');

                    DECLARE @ProveedorInsumosId INT = SCOPE_IDENTITY();

                    UPDATE Productos
                    SET ProveedorId = CASE
                        WHEN Nombre IN (
                            N'Amoxicilina 250 mg',
                            N'Vacuna antirrábica'
                        ) THEN @ProveedorId
                        WHEN Nombre = N'Jeringa 3 ml'
                            THEN @ProveedorInsumosId
                        ELSE 0
                    END;

                    IF EXISTS (
                        SELECT 1 FROM Productos
                        WHERE ProveedorId = 0
                    )
                    BEGIN
                        INSERT INTO Proveedores (Nombre)
                        VALUES (N'Proveedor pendiente de asignar');

                        DECLARE @ProveedorPendienteId INT = SCOPE_IDENTITY();

                        UPDATE Productos
                        SET ProveedorId = @ProveedorPendienteId
                        WHERE ProveedorId = 0;
                    END
                END
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_ProveedorId",
                table: "Productos",
                column: "ProveedorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Proveedores_ProveedorId",
                table: "Productos",
                column: "ProveedorId",
                principalTable: "Proveedores",
                principalColumn: "ProveedorId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Proveedores_ProveedorId",
                table: "Productos");

            migrationBuilder.DropTable(
                name: "Proveedores");

            migrationBuilder.DropIndex(
                name: "IX_Productos_ProveedorId",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "ProveedorId",
                table: "Productos");
        }
    }
}