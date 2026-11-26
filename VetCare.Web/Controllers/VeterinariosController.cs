using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Modelos;
using VetCare.Web.Proxies;

namespace VetCare.Web.Controllers;

public class VeterinariosController : Controller
{
    private readonly VeterinarioProxy _api;

    public VeterinariosController(VeterinarioProxy api) => _api = api;

    // HU-13: listado con filtro por especialidad (todos los roles pueden consultar)
    public async Task<IActionResult> Index(string? especialidad)
    {
        ViewBag.Especialidades = new SelectList(await _api.ListarEspecialidadesAsync(), especialidad);
        ViewBag.Especialidad = especialidad;
        return View(await _api.ListarAsync(especialidad));
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var veterinario = await _api.ObtenerAsync(id.Value);
        if (veterinario is null) return NotFound();
        return View(veterinario);
    }

    // HU-12: registrar (solo administrador)
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create()
    {
        await CargarUsuariosAsync();
        return View(new Veterinario
        {
            HoraInicio = new TimeSpan(8, 0, 0),
            HoraFin = new TimeSpan(16, 0, 0)
        });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create(
        [Bind("UsuarioId,CodigoColegiado,Nombre,Especialidad,Telefono,HoraInicio,HoraFin")] Veterinario veterinario)
    {
        if (!ModelState.IsValid)
        {
            await CargarUsuariosAsync();
            return View(veterinario);
        }

        var resultado = await _api.RegistrarAsync(veterinario);
        if (!resultado.EsExito)
        {
            ModelState.AddModelError(resultado.Campo ?? string.Empty, resultado.Mensaje ?? "Error");
            await CargarUsuariosAsync();
            return View(veterinario);
        }

        TempData["Exito"] = "Veterinario registrado correctamente";
        return RedirectToAction(nameof(Index));
    }

    // HU-14: editar (solo administrador)
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var veterinario = await _api.ObtenerAsync(id.Value);
        if (veterinario is null) return NotFound();

        await CargarUsuariosAsync(id);
        return View(veterinario);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Edit(int id,
        [Bind("VeterinarioId,UsuarioId,CodigoColegiado,Nombre,Especialidad,Telefono,HoraInicio,HoraFin,Activo")] Veterinario veterinario)
    {
        if (id != veterinario.VeterinarioId) return NotFound();

        if (!ModelState.IsValid)
        {
            await CargarUsuariosAsync(id);
            return View(veterinario);
        }

        var resultado = await _api.ActualizarAsync(id, veterinario);
        if (resultado.Tipo == TipoResultado.NoEncontrado) return NotFound();
        if (!resultado.EsExito)
        {
            ModelState.AddModelError(resultado.Campo ?? string.Empty, resultado.Mensaje ?? "Error");
            await CargarUsuariosAsync(id);
            return View(veterinario);
        }

        TempData["Exito"] = "Veterinario actualizado correctamente";
        return RedirectToAction(nameof(Index));
    }

    // HU-15: eliminar (solo administrador)
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var veterinario = await _api.ObtenerAsync(id.Value);
        if (veterinario is null) return NotFound();
        return View(veterinario);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken, Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var resultado = await _api.EliminarAsync(id);
        if (resultado.Tipo == TipoResultado.NoEncontrado) return NotFound();
        if (!resultado.EsExito)
        {
            TempData["Error"] = resultado.Mensaje;   // RN-06
            return RedirectToAction(nameof(Index));
        }

        TempData["Exito"] = "Veterinario eliminado correctamente";
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarUsuariosAsync(int? incluirVeterinarioId = null)
    {
        ViewBag.Usuarios = new SelectList(
            await _api.ListarUsuariosAsync(incluirVeterinarioId), "Id", "Texto");
    }
}