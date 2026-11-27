using VetCare.Abstracciones.Modelos;

namespace VetCare.Web.Proxies
{
    public interface IReporteProxy
    {
        Task<List<ReporteCita>> ObtenerReporteCitasAsync(
            FiltroReporteCita filtro);

        Task<ResumenReporteCitas> ObtenerResumenAsync(
            FiltroReporteCita filtro);

        Task<byte[]> ExportarCsvAsync(
            FiltroReporteCita filtro);

        Task<byte[]> ExportarExcelAsync(
            FiltroReporteCita filtro);
    }
}