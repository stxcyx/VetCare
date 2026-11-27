using System.Globalization;
using System.Net.Http.Json;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Web.Proxies
{
    public class ReporteProxy : IReporteProxy
    {
        private readonly HttpClient _httpClient;

        public ReporteProxy(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ReporteCita>> ObtenerReporteCitasAsync(
            FiltroReporteCita filtro)
        {
            var url = ConstruirUrl(
                "api/reportes/citas", filtro);

            var resultado = await _httpClient
                .GetFromJsonAsync<List<ReporteCita>>(url);

            return resultado ?? new List<ReporteCita>();
        }

        public async Task<ResumenReporteCitas> ObtenerResumenAsync(
            FiltroReporteCita filtro)
        {
            var url = ConstruirUrl(
                "api/reportes/resumen", filtro);

            var resultado = await _httpClient
                .GetFromJsonAsync<ResumenReporteCitas>(url);

            return resultado ?? throw new InvalidOperationException(
                "No se pudo obtener el resumen de citas.");
        }

        public async Task<byte[]> ExportarCsvAsync(
            FiltroReporteCita filtro)
        {
            var url = ConstruirUrl(
                "api/reportes/exportar-csv", filtro);

            using var respuesta = await _httpClient
                .GetAsync(url);

            respuesta.EnsureSuccessStatusCode();

            return await respuesta.Content
                .ReadAsByteArrayAsync();
        }

        public async Task<byte[]> ExportarExcelAsync(
            FiltroReporteCita filtro)
        {
            var url = ConstruirUrl(
                "api/reportes/exportar-excel", filtro);

            using var respuesta = await _httpClient
                .GetAsync(url);

            respuesta.EnsureSuccessStatusCode();

            return await respuesta.Content
                .ReadAsByteArrayAsync();
        }

        private static string ConstruirUrl(
            string ruta, FiltroReporteCita filtro)
        {
            var parametros = new List<string>();

            if (filtro.FechaInicio.HasValue)
            {
                parametros.Add(
                    $"fechaInicio={filtro.FechaInicio.Value.ToString(
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture)}");
            }

            if (filtro.FechaFin.HasValue)
            {
                parametros.Add(
                    $"fechaFin={filtro.FechaFin.Value.ToString(
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture)}");
            }

            if (filtro.VeterinarioId.HasValue)
            {
                parametros.Add(
                    $"veterinarioId={filtro.VeterinarioId.Value}");
            }

            return parametros.Count > 0
                ? $"{ruta}?{string.Join("&", parametros)}"
                : ruta;
        }
    }
}