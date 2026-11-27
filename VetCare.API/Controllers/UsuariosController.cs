using Microsoft.AspNetCore.Mvc;
using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Interfaces;
using VetCare.Abstracciones.Modelos;

namespace VetCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioFlujo _flujo;

    public UsuariosController(IUsuarioFlujo flujo) => _flujo = flujo;

    // GET api/usuarios?rol=Veterinario
    [HttpGet]
    public async Task<ActionResult<List<UsuarioCuenta>>> Listar(string? rol) =>
        Ok(await _flujo.ListarAsync(rol));

    // GET api/usuarios/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<UsuarioCuenta>> Obtener(string id)
    {
        var usuario = await _flujo.ObtenerAsync(id);
        if (usuario is null) return NotFound(new ErrorRespuesta { Mensaje = "Registro no encontrado" });
        return Ok(usuario);
    }

    // POST api/usuarios
    [HttpPost]
    public async Task<ActionResult<UsuarioCuenta>> Crear(CrearUsuarioDto dto)
    {
        var resultado = await _flujo.CrearAsync(dto);
        if (!resultado.EsExito) return Error(resultado);
        return CreatedAtAction(nameof(Obtener), new { id = resultado.Datos!.Id }, resultado.Datos);
    }

    // PUT api/usuarios/{id}/rol
    [HttpPut("{id}/rol")]
    public async Task<IActionResult> CambiarRol(string id, CambiarRolDto dto)
    {
        var resultado = await _flujo.CambiarRolAsync(id, dto.Rol);
        if (!resultado.EsExito) return Error(resultado);
        return NoContent();
    }

    // PUT api/usuarios/{id}/estado
    [HttpPut("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(string id, CambiarEstadoDto dto)
    {
        var resultado = await _flujo.CambiarEstadoAsync(id, dto.Activo);
        if (!resultado.EsExito) return Error(resultado);
        return NoContent();
    }

    private ObjectResult Error<T>(Resultado<T> resultado)
    {
        var cuerpo = new ErrorRespuesta { Campo = resultado.Campo, Mensaje = resultado.Mensaje ?? "Error" };
        if (resultado.Tipo == TipoResultado.NoEncontrado) return NotFound(cuerpo);
        return Conflict(cuerpo);
    }
}
