using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using VetCare.Abstracciones.Interfaces;
using VetCare.Abstracciones.Modelos;

namespace VetCare.DA.Repositorios;

public class ReporteDA : IReporteDA
{
    private readonly ApplicationDbContext _context;

    public ReporteDA(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReporteCita>> ObtenerReporteCitasAsync(
        FiltroReporteCita filtro)
    {
        var citas = new List<ReporteCita>();

        var conn = (SqlConnection)_context.Database.GetDbConnection();

        await _context.Database.OpenConnectionAsync();

        try
        {
            await using var cmd = new SqlCommand(
                "SP_Reporte_Citas", conn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@FechaInicio", SqlDbType.Date).Value =
                (object?)filtro.FechaInicio?.Date ?? DBNull.Value;

            cmd.Parameters.Add("@FechaFin", SqlDbType.Date).Value =
                (object?)filtro.FechaFin?.Date ?? DBNull.Value;

            cmd.Parameters.Add("@VeterinarioId", SqlDbType.Int).Value =
                (object?)filtro.VeterinarioId ?? DBNull.Value;

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                citas.Add(new ReporteCita
                {
                    CitaId = reader.GetInt32(
                        reader.GetOrdinal("CitaId")),

                    FechaHora = reader.GetDateTime(
                        reader.GetOrdinal("FechaHora")),

                    Cliente = reader.GetString(
                        reader.GetOrdinal("Cliente")),

                    Mascota = reader.GetString(
                        reader.GetOrdinal("Mascota")),

                    Veterinario = reader.GetString(
                        reader.GetOrdinal("Veterinario")),

                    Motivo = reader.GetString(
                        reader.GetOrdinal("Motivo")),

                    Estado = reader.GetString(
                        reader.GetOrdinal("Estado"))
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