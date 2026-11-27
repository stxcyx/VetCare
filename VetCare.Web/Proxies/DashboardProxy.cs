using System.Net.Http.Json;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Web.Proxies;

public class DashboardProxy : IDashboardProxy
{
    private readonly HttpClient _http;

    public DashboardProxy(HttpClient http)
    {
        _http = http;
    }

    public async Task<ResumenDashboard> ObtenerResumenAsync()
    {
        var resultado = await _http.GetFromJsonAsync<ResumenDashboard>(
            "api/dashboard/resumen");

        return resultado
            ?? throw new InvalidOperationException(
                "No se pudo obtener el resumen del dashboard.");
    }

    public async Task<List<CitasPorEstado>> ObtenerCitasPorEstadoAsync()
    {
        var resultado = await _http.GetFromJsonAsync<List<CitasPorEstado>>(
            "api/dashboard/citas-por-estado");

        return resultado
            ?? throw new InvalidOperationException(
                "No se pudieron obtener las citas por estado.");
    }

    public async Task<List<CitasPorVeterinario>> ObtenerCitasPorVeterinarioAsync()
    {
        var resultado = await _http.GetFromJsonAsync<List<CitasPorVeterinario>>(
            "api/dashboard/citas-por-veterinario");

        return resultado
            ?? throw new InvalidOperationException(
                "No se pudieron obtener las citas por veterinario.");
    }
}