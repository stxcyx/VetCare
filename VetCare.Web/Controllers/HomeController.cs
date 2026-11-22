using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetCare.Web.Models;

namespace VetCare.Web.Controllers;

public class HomeController : Controller
{
    // La página de inicio es el listado de clientes
    public IActionResult Index() => RedirectToAction("Index", "Clientes");

    public IActionResult Privacy() => View();

    [AllowAnonymous]
    public IActionResult AccesoDenegado() => View();

    [AllowAnonymous]
    public IActionResult Estado(int id)
    {
        ViewBag.Mensaje = id == 404
            ? "Registro no encontrado"
            : "Ocurrió un problema al procesar la solicitud";
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}