using Microsoft.AspNetCore.Mvc;
using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Interfaces;
using VetCare.Abstracciones.Modelos;

namespace VetCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly IClienteFlujo _flujo;
    public ClientesController(IClienteFlujo flujo) => _flujo = flujo;

    // GET api/clientes?buscar=Rojas&pagina=1
    [HttpGet]
    public async Task<ActionResult<PaginaResultado<Cliente>>> Listar(string? buscar, int pagina = 1)
    {
        return Ok(await _flujo.ListarAsync(buscar, pagina));
    }

    // GET api/clientes/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Cliente>> Obtener(int id)
    {
        var cliente = await _flujo.ObtenerAsync(id);
        if (cliente is null) return NotFound(new ErrorRespuesta { Mensaje = "Registro no encontrado" });
        return Ok(cliente);
    }

    // POST api/clientes
    [HttpPost]
    public async Task<ActionResult<Cliente>> Registrar(Cliente cliente)
    {
        var resultado = await _flujo.RegistrarAsync(cliente);
        if (!resultado.EsExito) return Error(resultado);
        return CreatedAtAction(nameof(Obtener), new { id = resultado.Datos!.ClienteId }, resultado.Datos);
    }

    // PUT api/clientes/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, Cliente cliente)
    {
        var resultado = await _flujo.ActualizarAsync(id, cliente);
        if (!resultado.EsExito) return Error(resultado);
        return NoContent();
    }

    // DELETE api/clientes/5
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