using Microsoft.AspNetCore.Mvc;
using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Interfaces;
using VetCare.Abstracciones.Modelos;

namespace VetCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MascotasController : ControllerBase
{
    private readonly IMascotaFlujo _flujo;
    public MascotasController(IMascotaFlujo flujo) => _flujo = flujo;

    // GET api/mascotas?buscar=Ana&especieId=2
    [HttpGet]
    public async Task<ActionResult<List<Mascota>>> Listar(string? buscar, int? especieId)
    {
        return Ok(await _flujo.ListarAsync(buscar, especieId));
    }

    // GET api/mascotas/5  (incluye el historial de citas)
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Mascota>> Obtener(int id)
    {
        var mascota = await _flujo.ObtenerAsync(id);
        if (mascota is null) return NotFound(new ErrorRespuesta { Mensaje = "Registro no encontrado" });
        return Ok(mascota);
    }

    // GET api/mascotas/especies  (para las listas desplegables)
    [HttpGet("especies")]
    public async Task<ActionResult<List<Especie>>> Especies()
    {
        return Ok(await _flujo.ListarEspeciesAsync());
    }

    // GET api/mascotas/duenos  (para la lista desplegable de dueño)
    [HttpGet("duenos")]
    public async Task<ActionResult<List<Cliente>>> Duenos()
    {
        return Ok(await _flujo.ListarDuenosAsync());
    }

    // POST api/mascotas
    [HttpPost]
    public async Task<ActionResult<Mascota>> Registrar(Mascota mascota)
    {
        var resultado = await _flujo.RegistrarAsync(mascota);
        if (!resultado.EsExito) return Error(resultado);
        return CreatedAtAction(nameof(Obtener), new { id = resultado.Datos!.MascotaId }, resultado.Datos);
    }

    // PUT api/mascotas/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, Mascota mascota)
    {
        var resultado = await _flujo.ActualizarAsync(id, mascota);
        if (!resultado.EsExito) return Error(resultado);
        return NoContent();
    }

    // DELETE api/mascotas/5  -> responde true si se desactivó, false si se eliminó
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<bool>> Eliminar(int id)
    {
        var resultado = await _flujo.EliminarAsync(id);
        if (!resultado.EsExito) return Error(resultado);
        return Ok(resultado.Datos);
    }

    private ObjectResult Error<T>(Resultado<T> resultado)
    {
        var cuerpo = new ErrorRespuesta { Campo = resultado.Campo, Mensaje = resultado.Mensaje ?? "Error" };
        if (resultado.Tipo == TipoResultado.NoEncontrado) return NotFound(cuerpo);
        return Conflict(cuerpo);
    }
}