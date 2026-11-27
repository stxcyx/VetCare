using VetCare.Abstracciones.Modelos;

namespace VetCare.Abstracciones.Interfaces;

public interface IReporteDA
{
    Task<List<ReporteCita>> ObtenerReporteCitasAsync(
        FiltroReporteCita filtro);
}