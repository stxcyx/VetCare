using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Modelos;
using VetCare.Web.Proxies;

namespace VetCare.Web.Controllers;

public class CitasController : Controller
{
    private const string MsgNoModificable = "La cita no se puede modificar en su estado actual";

    private readonly CitaProxy _api;

    public CitasController(CitaProxy api) => _api = api;

    // HU-17: agenda. Por defecto muestra las citas de hoy; "todas" quita el filtro de fecha.
    public async Task<IActionResult> Index(DateTime? fecha, int? veterinarioId, EstadoCita? estado, bool todas = false)
    {
        var esVeterinario = User.IsInRole("Veterinario");

        // HU-17 CA-02: un veterinario ve por defecto solo sus propias citas
        if (esVeterinario)
        {
            var miId = await _api.ObtenerVeterinarioIdPorUsuarioAsync(
                User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty);
            veterinarioId = miId ?? -1;   // sin ficha de veterinario: no ve citas
        }

        if (todas) fecha = null;
        else fecha ??= DateTime.Today;   // HU-17 CA-01

        ViewBag.Fecha = fecha;
        ViewBag.Todas = todas;
        ViewBag.VeterinarioId = veterinarioId;
        ViewBag.EsVeterinario = esVeterinario;
        ViewBag.Veterinarios = new SelectList(await _api.ListarVeterinariosAsync(), "VeterinarioId", "Nombre", veterinarioId);
        ViewBag.Estados = new SelectList(Enum.GetNames<EstadoCita>(), estado?.ToString());

        return View(await _api.ListarAsync(fecha, veterinarioId, estado));
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var cita = await _api.ObtenerAsync(id.Value);
        if (cita is null) return NotFound();
        return View(cita);
    }

    // HU-16: agendar (administrador y recepcionista)
    [Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> Create(int? mascotaId)
    {
        await CargarListasAsync();
        return View(new Cita
        {
            MascotaId = mascotaId ?? 0,
            FechaHora = DateTime.Today.AddDays(1).AddHours(9)
        });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> Create(
        [Bind("MascotaId,VeterinarioId,FechaHora,Motivo")] Cita cita)
    {
        if (!ModelState.IsValid)
        {
            await CargarListasAsync();
            return View(cita);
        }

        var resultado = await _api.AgendarAsync(cita);
        if (!resultado.EsExito)
        {
            ModelState.AddModelError(resultado.Campo ?? string.Empty, resultado.Mensaje ?? "Error");
            await CargarListasAsync();
            return View(cita);
        }

        TempData["Exito"] = "Cita agendada correctamente";
        return RedirectToAction(nameof(Index), new { fecha = cita.FechaHora.Date.ToString("yyyy-MM-dd") });
    }

    // HU-18: reprogramar
    [Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> Reprogramar(int? id)
    {
        if (id is null) return NotFound();
        var cita = await _api.ObtenerAsync(id.Value);
        if (cita is null) return NotFound();

        // HU-18 CA-03: la ruta directa de una cita Atendida o Cancelada no se permite
        if (cita.Estado != EstadoCita.Programada)
        {
            TempData["Error"] = MsgNoModificable;
            return RedirectToAction(nameof(Details), new { id });
        }

        return View(cita);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> Reprogramar(int id, DateTime fechaHora)
    {
        var resultado = await _api.ReprogramarAsync(id, fechaHora);
        if (resultado.Tipo == TipoResultado.NoEncontrado) return NotFound();

        if (!resultado.EsExito)
        {
            var cita = await _api.ObtenerAsync(id);
            if (cita is null) return NotFound();
            cita.FechaHora = fechaHora;   // conserva lo que escribió el usuario
            ModelState.AddModelError(resultado.Campo ?? string.Empty, resultado.Mensaje ?? "Error");
            return View(cita);
        }

        TempData["Exito"] = "Cita reprogramada correctamente";
        return RedirectToAction(nameof(Index), new { fecha = fechaHora.Date.ToString("yyyy-MM-dd") });
    }

    // HU-18: cancelar con motivo
    [Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> Cancelar(int? id)
    {
        if (id is null) return NotFound();
        var cita = await _api.ObtenerAsync(id.Value);
        if (cita is null) return NotFound();

        if (cita.Estado != EstadoCita.Programada)
        {
            TempData["Error"] = MsgNoModificable;
            return RedirectToAction(nameof(Details), new { id });
        }

        return View(cita);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> Cancelar(int id, string? motivoCancelacion)
    {
        var resultado = await _api.CancelarAsync(id, motivoCancelacion ?? string.Empty);
        if (resultado.Tipo == TipoResultado.NoEncontrado) return NotFound();

        if (!resultado.EsExito)
        {
            var cita = await _api.ObtenerAsync(id);
            if (cita is null) return NotFound();
            cita.MotivoCancelacion = motivoCancelacion;
            ModelState.AddModelError(resultado.Campo ?? string.Empty, resultado.Mensaje ?? "Error");
            return View(cita);
        }

        TempData["Exito"] = "Cita cancelada correctamente";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task CargarListasAsync()
    {
        var mascotas = (await _api.ListarMascotasAsync())
            .Select(m => new { m.MascotaId, Texto = $"{m.Nombre} ({m.Cliente?.Nombre} {m.Cliente?.Apellidos})" });
        ViewBag.Mascotas = new SelectList(mascotas, "MascotaId", "Texto");

        var veterinarios = (await _api.ListarVeterinariosAsync())
            .Select(v => new
            {
                v.VeterinarioId,
                Texto = $"{v.Nombre} · {v.Especialidad} ({v.HoraInicio:hh\\:mm}–{v.HoraFin:hh\\:mm})"
            });
        ViewBag.Veterinarios = new SelectList(veterinarios, "VeterinarioId", "Texto");
    }
}
