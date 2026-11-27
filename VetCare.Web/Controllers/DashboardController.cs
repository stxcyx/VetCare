using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetCare.Web.Proxies;

namespace VetCare.Web.Controllers;

[Authorize(Roles = "Administrador")]
public class DashboardController : Controller
{
    private readonly IDashboardProxy _proxy;

    public DashboardController(IDashboardProxy proxy)
    {
        _proxy = proxy;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var resumen = await _proxy.ObtenerResumenAsync();

        ViewBag.CitasPorEstado =
            await _proxy.ObtenerCitasPorEstadoAsync();

        ViewBag.CitasPorVeterinario =
            await _proxy.ObtenerCitasPorVeterinarioAsync();

        return View(resumen);
    }
}