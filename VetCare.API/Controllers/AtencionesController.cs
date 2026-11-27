using Microsoft.AspNetCore.Mvc;
using VetCare.Abstracciones.Interfaces;
using VetCare.Abstracciones.Modelos;

namespace VetCare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AtencionesController : ControllerBase
    {
        private readonly IAtencionFlujo _flujo;

        public AtencionesController(IAtencionFlujo flujo)
        {
            _flujo = flujo;
        }

        [HttpGet("pendientes")]
        [ProducesResponseType(typeof(List<Cita>),
            StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarPendientes()
        {
            var resultado = await _flujo
                .ListarPendientesAsync();

            return Ok(resultado);
        }

        [HttpGet("historial")]
        [ProducesResponseType(typeof(List<Cita>),
            StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarHistorial()
        {
            var resultado = await _flujo
                .ListarHistorialAsync();

            return Ok(resultado);
        }

        [HttpGet("{id:int}/productos")]
        [ProducesResponseType(typeof(List<ProductoAtencionDetalle>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ListarProductos(
            [FromRoute] int id)
        {
            var cita = await _flujo.ObtenerAsync(id);

            if (cita == null)
            {
                return NotFound(new
                {
                    mensaje = "La cita no existe."
                });
            }

            if (cita.Estado != EstadoCita.Atendida)
            {
                return Conflict(new
                {
                    mensaje = "La cita todavía no ha sido atendida."
                });
            }

            var resultado = await _flujo
                .ListarProductosAsync(id);

            return Ok(resultado);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Cita),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Obtener(
            [FromRoute] int id)
        {
            var resultado = await _flujo.ObtenerAsync(id);

            if (resultado == null)
            {
                return NotFound(new
                {
                    mensaje = "La cita no existe."
                });
            }

            return Ok(resultado);
        }

        [HttpPost("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Registrar(
            [FromRoute] int id,
            [FromBody] RegistroAtencion atencion)
        {
            var mensaje = await _flujo
                .RegistrarAsync(id, atencion);

            if (mensaje == "La cita no existe.")
            {
                return NotFound(new
                {
                    mensaje
                });
            }

            if (mensaje != null)
            {
                return Conflict(new
                {
                    mensaje
                });
            }

            return Ok(new
            {
                mensaje = "Atención registrada correctamente."
            });
        }
    }
}