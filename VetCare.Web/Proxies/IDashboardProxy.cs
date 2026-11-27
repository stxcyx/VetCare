using VetCare.Abstracciones.Modelos;

namespace VetCare.Web.Proxies;

public interface IDashboardProxy
{
    Task<ResumenDashboard> ObtenerResumenAsync();

    Task<List<CitasPorEstado>> ObtenerCitasPorEstadoAsync();

    Task<List<CitasPorVeterinario>> ObtenerCitasPorVeterinarioAsync();
}