using VetCare.Abstracciones.Modelos;

namespace VetCare.Abstracciones.Interfaces.DA;

public interface IDashboardDA
{
    Task<ResumenDashboard> ObtenerResumenAsync();

    Task<List<CitasPorEstado>> ObtenerCitasPorEstadoAsync();

    Task<List<CitasPorVeterinario>> ObtenerCitasPorVeterinarioAsync();
}