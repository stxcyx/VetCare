using Microsoft.AspNetCore.Mvc;
using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Interfaces;
using VetCare.Abstracciones.Modelos;

namespace VetCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VeterinariosController : ControllerBase
{
    private readonly IVeterinarioFlujo _flujo;

    public VeterinariosController(IVeterinarioFlujo flujo) => _flujo = flujo;

    // GET api/veterinarios?especialidad=Cirugía&soloActivos=true
    [HttpGet]
    public async Task<ActionResult<List<Veterinario>>> Listar(string? especialidad, bool soloActivos = false)
    {
        return Ok(await _flujo.ListarAsync(especialidad, soloActivos));
    }

    // GET api/veterinarios/especialidades  (para el filtro del listado)
    [HttpGet("especialidades")]
    public async Task<ActionResult<List<string>>> Especialidades()
    {
        return Ok(await _flujo.ListarEspecialidadesAsync());
    }

    // GET api/veterinarios/usuarios?incluirVeterinarioId=2  (lista desplegable de cuentas)
    [HttpGet("usuarios")]
    public async Task<ActionResult<List<UsuarioOpcion>>> Usuarios(int? incluirVeterinarioId)
    {
        return Ok(await _flujo.ListarUsuariosDisponiblesAsync(incluirVeterinarioId));
    }

    // GET api/veterinarios/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Veterinario>> Obtener(int id)
    {
        var veterinario = await _flujo.ObtenerAsync(id);
        if (veterinario is null) return NotFound(new ErrorRespuesta { Mensaje = "Registro no encontrado" });
        return Ok(veterinario);
    }

    // POST api/veterinarios
    [HttpPost]
    public async Task<ActionResult<Veterinario>> Registrar(Veterinario veterinario)
    {
        var resultado = await _flujo.RegistrarAsync(veterinario);
        if (!resultado.EsExito) return Error(resultado);
        return CreatedAtAction(nameof(Obtener), new { id = resultado.Datos!.VeterinarioId }, resultado.Datos);
    }

    // PUT api/veterinarios/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, Veterinario veterinario)
    {
        var resultado = await _flujo.ActualizarAsync(id, veterinario);
        if (!resultado.EsExito) return Error(resultado);
        return NoContent();
    }

    // DELETE api/veterinarios/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var resultado = await _flujo.EliminarAsync(id);
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