using VetCare.Abstracciones.Modelos;

namespace VetCare.Abstracciones.Interfaces;

public interface IDashboardFlujo
{
    Task<ResumenDashboard> ObtenerResumenAsync();

    Task<List<CitasPorEstado>> ObtenerCitasPorEstadoAsync();

    Task<List<CitasPorVeterinario>> ObtenerCitasPorVeterinarioAsync();
}