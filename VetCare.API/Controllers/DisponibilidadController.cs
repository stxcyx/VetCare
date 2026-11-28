using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Interfaces;

namespace VetCare.API.Controllers;

// HU-26: horarios libres de un veterinario en una fecha
[ApiController]
[Route("api/veterinarios/{veterinarioId:int}/disponibilidad")]
public class DisponibilidadController : ControllerBase
{
    private readonly ICitaFlujo _flujo;

    public DisponibilidadController(ICitaFlujo flujo) => _flujo = flujo;

    // GET api/veterinarios/2/disponibilidad?fecha=2026-12-01
    [HttpGet]
    public async Task<ActionResult<List<BloqueHorario>>> Obtener(int veterinarioId, string? fecha)
    {
        // CA-02: fecha ausente o con formato inválido -> 400
        if (!DateOnly.TryParseExact(fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var dia))
            return BadRequest(new ErrorRespuesta
            {
                Campo = "fecha",
                Mensaje = "La fecha no es válida; use el formato aaaa-MM-dd"
            });

        var resultado = await _flujo.ObtenerDisponibilidadAsync(veterinarioId, dia);
        if (resultado.Tipo == TipoResultado.NoEncontrado)   // CA-02: veterinario inexistente -> 404
            return NotFound(new ErrorRespuesta { Mensaje = resultado.Mensaje ?? "Veterinario no encontrado" });
        if (!resultado.EsExito)
            return Conflict(new ErrorRespuesta { Campo = resultado.Campo, Mensaje = resultado.Mensaje ?? "Error" });

        return Ok(resultado.Datos);   // CA-01
    }
}
