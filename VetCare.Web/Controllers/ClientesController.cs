using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Modelos;
using VetCare.Web.Proxies;

namespace VetCare.Web.Controllers;

public class ClientesController : Controller
{
    private readonly ClienteProxy _api;
    public ClientesController(ClienteProxy api) => _api = api;

    public async Task<IActionResult> Index(string? buscar, int pagina = 1)
    {
        var resultado = await _api.ListarAsync(buscar, pagina);
        ViewBag.Buscar = buscar;
        ViewBag.Pagina = resultado.Pagina;
        ViewBag.TotalPaginas = resultado.TotalPaginas;
        return View(resultado.Elementos);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var cliente = await _api.ObtenerAsync(id.Value);
        if (cliente is null) return NotFound();
        return View(cliente);
    }

    [Authorize(Roles = "Administrador,Recepcionista")]
    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> Create(
        [Bind("Cedula,Nombre,Apellidos,Telefono,Correo,Direccion")] Cliente cliente)
    {
        if (!ModelState.IsValid) return View(cliente);

        cliente.UsuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);   // quién lo registró
        var resultado = await _api.RegistrarAsync(cliente);
        if (!resultado.EsExito)
        {
            ModelState.AddModelError(resultado.Campo ?? string.Empty, resultado.Mensaje ?? "Error");
            return View(cliente);
        }

        TempData["Exito"] = "Cliente registrado correctamente";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var cliente = await _api.ObtenerAsync(id.Value);
        if (cliente is null) return NotFound();
        return View(cliente);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> Edit(int id,
        [Bind("ClienteId,Cedula,Nombre,Apellidos,Telefono,Correo,Direccion")] Cliente cliente)
    {
        if (id != cliente.ClienteId) return NotFound();
        if (!ModelState.IsValid) return View(cliente);

        var resultado = await _api.ActualizarAsync(id, cliente);
        if (resultado.Tipo == TipoResultado.NoEncontrado) return NotFound();
        if (!resultado.EsExito)
        {
            ModelState.AddModelError(resultado.Campo ?? string.Empty, resultado.Mensaje ?? "Error");
            return View(cliente);
        }

        TempData["Exito"] = "Cliente actualizado correctamente";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var cliente = await _api.ObtenerAsync(id.Value);
        if (cliente is null) return NotFound();
        return View(cliente);
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

        TempData["Exito"] = "Cliente eliminado correctamente";
        return RedirectToAction(nameof(Index));
    }
}