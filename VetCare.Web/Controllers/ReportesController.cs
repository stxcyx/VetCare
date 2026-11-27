using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VetCare.Abstracciones.Modelos;
using VetCare.Web.Proxies;

namespace VetCare.Web.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ReportesController : Controller
    {
        private readonly IReporteProxy _reporteProxy;
        private readonly VeterinarioProxy _veterinarioProxy;

        public ReportesController(
            IReporteProxy reporteProxy,
            VeterinarioProxy veterinarioProxy)
        {
            _reporteProxy = reporteProxy;
            _veterinarioProxy = veterinarioProxy;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            [FromQuery] FiltroReporteCita filtro)
        {
            ViewBag.Filtro = filtro;

            await CargarVeterinariosAsync(
                filtro.VeterinarioId);

            if (!ModelState.IsValid)
            {
                ViewBag.Resumen = new ResumenReporteCitas();

                return View(new List<ReporteCita>());
            }

            var resultado = await _reporteProxy
                .ObtenerReporteCitasAsync(filtro);

            ViewBag.Resumen = await _reporteProxy
                .ObtenerResumenAsync(filtro);

            return View(resultado);
        }

        [HttpGet]
        public async Task<IActionResult> ExportarCsv(
            [FromQuery] FiltroReporteCita filtro)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Filtro = filtro;
                ViewBag.Resumen = new ResumenReporteCitas();

                await CargarVeterinariosAsync(
                    filtro.VeterinarioId);

                return View(
                    "Index",
                    new List<ReporteCita>());
            }

            var archivo = await _reporteProxy
                .ExportarCsvAsync(filtro);

            var nombreArchivo =
                $"ReporteCitas_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            return File(
                archivo,
                "text/csv; charset=utf-8",
                nombreArchivo);
        }

        [HttpGet]
        public async Task<IActionResult> ExportarExcel(
            [FromQuery] FiltroReporteCita filtro)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Filtro = filtro;
                ViewBag.Resumen = new ResumenReporteCitas();

                await CargarVeterinariosAsync(
                    filtro.VeterinarioId);

                return View(
                    "Index",
                    new List<ReporteCita>());
            }

            var archivo = await _reporteProxy
                .ExportarExcelAsync(filtro);

            var nombreArchivo =
                $"ReporteCitas_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(
                archivo,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                nombreArchivo);
        }

        private async Task CargarVeterinariosAsync(
            int? veterinarioId)
        {
            var veterinarios = await _veterinarioProxy
                .ListarAsync(null);

            ViewBag.Veterinarios = new SelectList(
                veterinarios,
                nameof(Veterinario.VeterinarioId),
                nameof(Veterinario.Nombre),
                veterinarioId);
        }
    }
}