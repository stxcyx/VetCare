using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VetCare.Abstracciones.Comunes;
using VetCare.Web.Proxies;

namespace VetCare.Web.Controllers;

// Usuarios y roles: solo el administrador
[Authorize(Roles = "Administrador")]
public class UsuariosController : Controller
{
    private readonly UsuarioProxy _api;

    public UsuariosController(UsuarioProxy api) => _api = api;

    public async Task<IActionResult> Index(string? rol)
    {
        ViewBag.Roles = new SelectList(RolesSistema.Todos, rol);
        return View(await _api.ListarAsync(rol));
    }

    public IActionResult Create()
    {
        CargarRoles();
        return View(new CrearUsuarioDto());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CrearUsuarioDto dto)
    {
        if (!ModelState.IsValid)
        {
            CargarRoles(dto.Rol);
            return View(dto);
        }

        var resultado = await _api.CrearAsync(dto);
        if (!resultado.EsExito)
        {
            ModelState.AddModelError(resultado.Campo ?? string.Empty, resultado.Mensaje ?? "Error");
            CargarRoles(dto.Rol);
            return View(dto);
        }

        TempData["Exito"] = "Usuario creado correctamente";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> CambiarRol(string? id)
    {
        if (id is null) return NotFound();
        var cuenta = await _api.ObtenerAsync(id);
        if (cuenta is null) return NotFound();

        if (EsMiCuenta(id)) return Rechazar();

        ViewBag.Cuenta = cuenta;
        CargarRoles(cuenta.Rol);
        return View(new CambiarRolDto { Rol = cuenta.Rol });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarRol(string id, CambiarRolDto dto)
    {
        if (EsMiCuenta(id)) return Rechazar();

        var resultado = await _api.CambiarRolAsync(id, dto.Rol);
        if (resultado.Tipo == TipoResultado.NoEncontrado) return NotFound();
        if (!resultado.EsExito)
        {
            TempData["Error"] = resultado.Mensaje;
            return RedirectToAction(nameof(CambiarRol), new { id });
        }

        TempData["Exito"] = "Rol actualizado correctamente";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(string id, bool activo)
    {
        if (EsMiCuenta(id)) return Rechazar();

        var resultado = await _api.CambiarEstadoAsync(id, activo);
        if (resultado.Tipo == TipoResultado.NoEncontrado) return NotFound();

        if (resultado.EsExito)
            TempData["Exito"] = activo ? "Usuario activado correctamente" : "Usuario desactivado correctamente";
        else
            TempData["Error"] = resultado.Mensaje;

        return RedirectToAction(nameof(Index));
    }

    // El administrador no puede quitarse su propio rol ni desactivarse
    private bool EsMiCuenta(string id) => id == User.FindFirstValue(ClaimTypes.NameIdentifier);

    private IActionResult Rechazar()
    {
        TempData["Error"] = "No puede cambiar su propio rol ni desactivar su propia cuenta";
        return RedirectToAction(nameof(Index));
    }

    private void CargarRoles(string? seleccionado = null) =>
        ViewBag.Roles = new SelectList(RolesSistema.Todos, seleccionado);
}
