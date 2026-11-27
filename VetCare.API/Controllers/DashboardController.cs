using Microsoft.AspNetCore.Mvc;
using VetCare.Abstracciones.Interfaces;
using VetCare.Abstracciones.Modelos;

namespace VetCare.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardFlujo _flujo;

    public DashboardController(IDashboardFlujo flujo)
    {
        _flujo = flujo;
    }

    /// <summary>
    /// Obtiene los indicadores generales del dashboard.
    /// </summary>
    [HttpGet("resumen")]
    [ProducesResponseType(typeof(ResumenDashboard), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerResumen()
    {
        var resultado = await _flujo.ObtenerResumenAsync();

        return Ok(resultado);
    }

    /// <summary>
    /// Obtiene la cantidad de citas agrupadas por estado.
    /// </summary>
    [HttpGet("citas-por-estado")]
    [ProducesResponseType(typeof(List<CitasPorEstado>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerCitasPorEstado()
    {
        var resultado = await _flujo.ObtenerCitasPorEstadoAsync();

        return Ok(resultado);
    }

    /// <summary>
    /// Obtiene la cantidad de citas agrupadas por veterinario.
    /// </summary>
    [HttpGet("citas-por-veterinario")]
    [ProducesResponseType(typeof(List<CitasPorVeterinario>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerCitasPorVeterinario()
    {
        var resultado = await _flujo.ObtenerCitasPorVeterinarioAsync();

        return Ok(resultado);
    }
}