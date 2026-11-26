using Microsoft.AspNetCore.Mvc;
using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Interfaces;
using VetCare.Abstracciones.Modelos;

namespace VetCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly IProductoFlujo _flujo;
    public ProductosController(IProductoFlujo flujo) => _flujo = flujo;

    // GET api/productos?buscar=Amoxicilina&pagina=1
    [HttpGet]
    [ProducesResponseType(typeof(PaginaResultado<Producto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginaResultado<Producto>>> Listar(
        [FromQuery] string? buscar, [FromQuery] int pagina = 1)
    {
        return Ok(await _flujo.ListarAsync(buscar, pagina));
    }

    // GET api/productos/5
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Producto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Producto>> Obtener([FromRoute] int id)
    {
        var producto = await _flujo.ObtenerAsync(id);
        if (producto is null) return NotFound(new ErrorRespuesta { Mensaje = "Registro no encontrado" });
        return Ok(producto);
    }

    // POST api/productos
    [HttpPost]
    [ProducesResponseType(typeof(Producto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Producto>> Registrar([FromBody] Producto producto)
    {
        var resultado = await _flujo.RegistrarAsync(producto);
        if (!resultado.EsExito) return Error(resultado);
        return CreatedAtAction(nameof(Obtener), new { id = resultado.Datos!.ProductoId }, resultado.Datos);
    }

    // PUT api/productos/5
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar([FromRoute] int id, [FromBody] Producto producto)
    {
        var resultado = await _flujo.ActualizarAsync(id, producto);
        if (!resultado.EsExito) return Error(resultado);
        return NoContent();
    }

    // DELETE api/productos/5
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar([FromRoute] int id)
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