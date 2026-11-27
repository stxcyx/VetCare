using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.Json;
using VetCare.Abstracciones.Interfaces;
using VetCare.Abstracciones.Modelos;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetCare.DA.Repositorios
{
    public class AtencionDA : IAtencionDA
    {
        private readonly ApplicationDbContext _context;

        public AtencionDA(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Cita>> ListarPendientesAsync()
        {
            var citas = new List<Cita>();

            var conn = (SqlConnection)_context.Database
                .GetDbConnection();

            await _context.Database.OpenConnectionAsync();

            try
            {
                await using var cmd = new SqlCommand(
                    "SP_Atencion_ListarPendientes", conn);

                cmd.CommandType = CommandType.StoredProcedure;

                await using var reader =
                    await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    citas.Add(LeerCita(reader));
                }

                return citas;
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        public async Task<List<Cita>> ListarHistorialAsync()
        {
            var citas = new List<Cita>();

            var conn = (SqlConnection)_context.Database
                .GetDbConnection();

            await _context.Database.OpenConnectionAsync();

            try
            {
                await using var cmd = new SqlCommand(
                    "SP_Atencion_ListarHistorial", conn);

                cmd.CommandType = CommandType.StoredProcedure;

                await using var reader =
                    await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    citas.Add(LeerCita(reader));
                }

                return citas;
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        public async Task<List<ProductoAtencionDetalle>>
            ListarProductosAsync(int citaId)
        {
            var productos = new List<ProductoAtencionDetalle>();

            var conn = (SqlConnection)_context.Database
                .GetDbConnection();

            await _context.Database.OpenConnectionAsync();

            try
            {
                await using var cmd = new SqlCommand(
                    "SP_Atencion_ListarProductos", conn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(
                    "@CitaId", SqlDbType.Int).Value = citaId;

                await using var reader =
                    await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    productos.Add(new ProductoAtencionDetalle
                    {
                        ProductoId = reader.GetInt32(
                            reader.GetOrdinal("ProductoId")),

                        Nombre = reader.GetString(
                            reader.GetOrdinal("Nombre")),

                        Cantidad = reader.GetInt32(
                            reader.GetOrdinal("Cantidad"))
                    });
                }

                return productos;
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        public async Task<Cita?> ObtenerAsync(int citaId)
        {
            var conn = (SqlConnection)_context.Database
                .GetDbConnection();

            await _context.Database.OpenConnectionAsync();

            try
            {
                await using var cmd = new SqlCommand(
                    "SP_Atencion_Obtener", conn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(
                    "@CitaId", SqlDbType.Int).Value = citaId;

                await using var reader =
                    await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return LeerCita(reader);
                }

                return null;
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        public async Task<string?> RegistrarAsync(
            int citaId,
            RegistroAtencion atencion)
        {
            var conn = (SqlConnection)_context.Database
                .GetDbConnection();

            await _context.Database.OpenConnectionAsync();

            try
            {
                await using var cmd = new SqlCommand(
                    "SP_Atencion_Registrar", conn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(
                    "@CitaId", SqlDbType.Int).Value = citaId;

                cmd.Parameters.Add(
                    "@Diagnostico",
                    SqlDbType.NVarChar, 1000).Value =
                    atencion.Diagnostico;

                cmd.Parameters.Add(
                    "@Tratamiento",
                    SqlDbType.NVarChar, 1000).Value =
                    atencion.Tratamiento;

                cmd.Parameters.Add(
                    "@Productos",
                    SqlDbType.NVarChar, -1).Value =
                    JsonSerializer.Serialize(atencion.Productos);

                var resultado = await cmd.ExecuteScalarAsync();

                if (resultado == DBNull.Value)
                {
                    return null;
                }

                return resultado?.ToString()
                    ?? "No se pudo confirmar el registro de la atención.";
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        private static Cita LeerCita(SqlDataReader reader)
        {
            return new Cita
            {
                CitaId = reader.GetInt32(
                    reader.GetOrdinal("CitaId")),

                MascotaId = reader.GetInt32(
                    reader.GetOrdinal("MascotaId")),

                VeterinarioId = reader.GetInt32(
                    reader.GetOrdinal("VeterinarioId")),

                NombreMascota = reader.GetString(
                    reader.GetOrdinal("NombreMascota")),

                NombreVeterinario = reader.GetString(
                    reader.GetOrdinal("NombreVeterinario")),

                FechaHora = reader.GetDateTime(
                    reader.GetOrdinal("FechaHora")),

                DuracionMin = reader.GetInt32(
                    reader.GetOrdinal("DuracionMin")),

                Motivo = reader.GetString(
                    reader.GetOrdinal("Motivo")),

                Estado = Enum.Parse<EstadoCita>(
                    reader.GetString(
                        reader.GetOrdinal("Estado"))),

                Diagnostico =
                    reader["Diagnostico"] == DBNull.Value
                        ? null
                        : reader["Diagnostico"].ToString(),

                Tratamiento =
                    reader["Tratamiento"] == DBNull.Value
                        ? null
                        : reader["Tratamiento"].ToString(),

                MotivoCancelacion =
                    reader["MotivoCancelacion"] == DBNull.Value
                        ? null
                        : reader["MotivoCancelacion"].ToString()
            };
        }
    }
}