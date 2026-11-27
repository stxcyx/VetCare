using VetCare.Abstracciones.Interfaces;
using VetCare.Abstracciones.Interfaces.DA;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Flujo;

public class DashboardFlujo : IDashboardFlujo
{
    private readonly IDashboardDA _da;

    public DashboardFlujo(IDashboardDA da)
    {
        _da = da;
    }

    public async Task<ResumenDashboard> ObtenerResumenAsync()
    {
        return await _da.ObtenerResumenAsync();
    }

    public async Task<List<CitasPorEstado>> ObtenerCitasPorEstadoAsync()
    {
        return await _da.ObtenerCitasPorEstadoAsync();
    }

    public async Task<List<CitasPorVeterinario>> ObtenerCitasPorVeterinarioAsync()
    {
        return await _da.ObtenerCitasPorVeterinarioAsync();
    }
}