using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using VetCare.Abstracciones.Interfaces.DA;
using VetCare.Abstracciones.Modelos;

namespace VetCare.DA.Repositorios;

public class DashboardDA : IDashboardDA
{
    private readonly ApplicationDbContext _context;

    public DashboardDA(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ResumenDashboard> ObtenerResumenAsync()
    {
        var resumen = new ResumenDashboard();

        var conn = (SqlConnection)_context.Database.GetDbConnection();

        await _context.Database.OpenConnectionAsync();

        try
        {
            await using var cmd = new SqlCommand(
                "SP_Dashboard_Resumen", conn);

            cmd.CommandType = CommandType.StoredProcedure;

            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                resumen.CitasHoy =
                    reader.GetInt32(reader.GetOrdinal("CitasHoy"));

                resumen.CitasMes =
                    reader.GetInt32(reader.GetOrdinal("CitasMes"));

                resumen.MascotasActivas =
                    reader.GetInt32(reader.GetOrdinal("MascotasActivas"));

                resumen.ProductosStockBajo =
                    reader.GetInt32(reader.GetOrdinal("ProductosStockBajo"));
            }

            return resumen;
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }

    public async Task<List<CitasPorEstado>> ObtenerCitasPorEstadoAsync()
    {
        var citas = new List<CitasPorEstado>();

        var conn = (SqlConnection)_context.Database.GetDbConnection();

        await _context.Database.OpenConnectionAsync();

        try
        {
            await using var cmd = new SqlCommand(
                "SP_Dashboard_CitasPorEstado", conn);

            cmd.CommandType = CommandType.StoredProcedure;

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                citas.Add(new CitasPorEstado
                {
                    Estado = reader.GetString(
                        reader.GetOrdinal("Estado")),

                    Cantidad = reader.GetInt32(
                        reader.GetOrdinal("Cantidad"))
                });
            }

            return citas;
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }

    public async Task<List<CitasPorVeterinario>> ObtenerCitasPorVeterinarioAsync()
    {
        var citas = new List<CitasPorVeterinario>();

        var conn = (SqlConnection)_context.Database.GetDbConnection();

        await _context.Database.OpenConnectionAsync();

        try
        {
            await using var cmd = new SqlCommand(
                "SP_Dashboard_CitasPorVeterinario", conn);

            cmd.CommandType = CommandType.StoredProcedure;

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                citas.Add(new CitasPorVeterinario
                {
                    Veterinario = reader.GetString(
                        reader.GetOrdinal("Veterinario")),

                    Cantidad = reader.GetInt32(
                        reader.GetOrdinal("Cantidad"))
                });
            }

            return citas;
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }
}