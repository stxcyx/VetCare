using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Modelos;
using VetCare.Web.Proxies;

namespace VetCare.Web.Controllers;

public class MascotasController : Controller
{
    private readonly MascotaProxy _api;
    public MascotasController(MascotaProxy api) => _api = api;

    // HU-09: listado con filtro por especie y búsqueda 
    public async Task<IActionResult> Index(string? buscar, int? especieId)
    {
        ViewBag.Buscar = buscar;
        ViewBag.Especies = new SelectList(await _api.ListarEspeciesAsync(), "EspecieId", "Nombre", especieId);
        return View(await _api.ListarAsync(buscar, especieId));
    }

    // HU-09: ficha con edad e historial 
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var mascota = await _api.ObtenerAsync(id.Value);
        if (mascota is null) return NotFound();
        return View(mascota);
    }

    // HU-08: registrar.
    [Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> Create(int? clienteId)
    {
        await CargarListasAsync();
        return View(new Mascota { ClienteId = clienteId ?? 0, FechaNacimiento = DateTime.Today });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> Create(
        [Bind("ClienteId,EspecieId,Nombre,Raza,Sexo,FechaNacimiento,PesoKg")] Mascota mascota)
    {
        if (!ModelState.IsValid)
        {
            await CargarListasAsync();
            return View(mascota);
        }

        var resultado = await _api.RegistrarAsync(mascota);
        if (!resultado.EsExito)
        {
            ModelState.AddModelError(resultado.Campo ?? string.Empty, resultado.Mensaje ?? "Error");
            await CargarListasAsync();
            return View(mascota);
        }

        TempData["Exito"] = "Mascota registrada correctamente";
        // HU-08 CA-01: la mascota aparece en el detalle de su dueño
        return RedirectToAction("Details", "Clientes", new { id = mascota.ClienteId });
    }

    // HU-10: editar
    [Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var mascota = await _api.ObtenerAsync(id.Value);
        if (mascota is null) return NotFound();
        await CargarListasAsync();
        return View(mascota);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> Edit(int id,
        [Bind("MascotaId,ClienteId,EspecieId,Nombre,Raza,Sexo,FechaNacimiento,PesoKg")] Mascota mascota)
    {
        if (id != mascota.MascotaId) return NotFound();
        if (!ModelState.IsValid)
        {
            await CargarListasAsync();
            return View(mascota);
        }

        var resultado = await _api.ActualizarAsync(id, mascota);
        if (resultado.Tipo == TipoResultado.NoEncontrado) return NotFound();
        if (!resultado.EsExito)
        {
            ModelState.AddModelError(resultado.Campo ?? string.Empty, resultado.Mensaje ?? "Error");
            await CargarListasAsync();
            return View(mascota);
        }

        TempData["Exito"] = "Mascota actualizada correctamente";
        return RedirectToAction(nameof(Details), new { id });
    }

    // HU-11: eliminar o desactivar (solo administrador)
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var mascota = await _api.ObtenerAsync(id.Value);
        if (mascota is null) return NotFound();
        return View(mascota);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken, Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var resultado = await _api.EliminarAsync(id);
        if (resultado.Tipo == TipoResultado.NoEncontrado) return NotFound();

        TempData["Exito"] = resultado.Datos
            ? "La mascota tiene historial de citas, por eso quedó Inactiva"
            : "Mascota eliminada correctamente";
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarListasAsync()
    {
        var duenos = (await _api.ListarDuenosAsync())
            .Select(c => new { c.ClienteId, Texto = $"{c.Apellidos}, {c.Nombre} ({c.Cedula})" });
        ViewBag.Duenos = new SelectList(duenos, "ClienteId", "Texto");
        ViewBag.Especies = new SelectList(await _api.ListarEspeciesAsync(), "EspecieId", "Nombre");
    }
}