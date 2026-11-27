using Microsoft.AspNetCore.Mvc;
using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Interfaces;
using VetCare.Abstracciones.Modelos;

namespace VetCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CitasController : ControllerBase
{
    private readonly ICitaFlujo _flujo;

    public CitasController(ICitaFlujo flujo) => _flujo = flujo;

    // GET api/citas?fecha=2026-11-27&veterinarioId=1&estado=Programada
    [HttpGet]
    public async Task<ActionResult<List<Cita>>> Listar(DateTime? fecha, int? veterinarioId, EstadoCita? estado)
    {
        return Ok(await _flujo.ListarAsync(fecha, veterinarioId, estado));
    }

    // GET api/citas/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Cita>> Obtener(int id)
    {
        var cita = await _flujo.ObtenerAsync(id);
        if (cita is null) return NotFound(new ErrorRespuesta { Mensaje = "Registro no encontrado" });
        return Ok(cita);
    }

    // GET api/citas/mascotas  (lista desplegable del formulario)
    [HttpGet("mascotas")]
    public async Task<ActionResult<List<Mascota>>> Mascotas()
    {
        return Ok(await _flujo.ListarMascotasActivasAsync());
    }

    // GET api/citas/veterinarios  (solo activos, HU-14 CA-02)
    [HttpGet("veterinarios")]
    public async Task<ActionResult<List<Veterinario>>> Veterinarios()
    {
        return Ok(await _flujo.ListarVeterinariosActivosAsync());
    }

    // GET api/citas/veterinario-de-usuario?usuarioId=...  (HU-17 CA-02)
    [HttpGet("veterinario-de-usuario")]
    public async Task<ActionResult<int>> VeterinarioDeUsuario(string usuarioId)
    {
        var id = await _flujo.ObtenerVeterinarioIdPorUsuarioAsync(usuarioId);
        if (id is null) return NotFound(new ErrorRespuesta { Mensaje = "Registro no encontrado" });
        return Ok(id.Value);
    }

    // POST api/citas
    [HttpPost]
    public async Task<ActionResult<Cita>> Agendar(Cita cita)
    {
        var resultado = await _flujo.AgendarAsync(cita);
        if (!resultado.EsExito) return Error(resultado);
        return CreatedAtAction(nameof(Obtener), new { id = resultado.Datos!.CitaId }, resultado.Datos);
    }

    // PUT api/citas/5/reprogramar
    [HttpPut("{id:int}/reprogramar")]
    public async Task<IActionResult> Reprogramar(int id, ReprogramarCitaDto dto)
    {
        var resultado = await _flujo.ReprogramarAsync(id, dto.FechaHora);
        if (!resultado.EsExito) return Error(resultado);
        return NoContent();
    }

    // PUT api/citas/5/cancelar
    [HttpPut("{id:int}/cancelar")]
    public async Task<IActionResult> Cancelar(int id, CancelarCitaDto dto)
    {
        var resultado = await _flujo.CancelarAsync(id, dto.Motivo);
        if (!resultado.EsExito) return Error(resultado);
        return NoContent();
    }

    // resultado de Flujo a una respuesta HTTP: 404 o 409
    private ObjectResult Error<T>(Resultado<T> resultado)
    {
        var cuerpo = new ErrorRespuesta { Campo = resultado.Campo, Mensaje = resultado.Mensaje ?? "Error" };
        if (resultado.Tipo == TipoResultado.NoEncontrado) return NotFound(cuerpo);
        return Conflict(cuerpo);
    }
}
