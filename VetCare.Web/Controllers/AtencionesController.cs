using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetCare.Abstracciones.Modelos;
using VetCare.Web.Proxies;

namespace VetCare.Web.Controllers
{
    [Authorize(Roles = "Administrador,Veterinario")]
    public class AtencionesController : Controller
    {
        private readonly IAtencionProxy _atencionProxy;
        private readonly IProductoProxy _productoProxy;

        public AtencionesController(
            IAtencionProxy atencionProxy,
            IProductoProxy productoProxy)
        {
            _atencionProxy = atencionProxy;
            _productoProxy = productoProxy;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var citas = await _atencionProxy
                .ListarPendientesAsync();

            ViewBag.Historial = await _atencionProxy
                .ListarHistorialAsync();

            return View(citas);
        }

        [HttpGet]
        public async Task<IActionResult> Detalle(
            [FromRoute] int id)
        {
            var cita = await _atencionProxy
                .ObtenerAsync(id);

            if (cita == null)
            {
                return NotFound();
            }

            if (cita.Estado != EstadoCita.Atendida)
            {
                TempData["Error"] =
                    "La cita todavía no ha sido atendida.";

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Productos = await _atencionProxy
                .ListarProductosAsync(id);

            return View(cita);
        }

        [HttpGet]
        public async Task<IActionResult> Registrar(
            [FromRoute] int id)
        {
            var cita = await _atencionProxy
                .ObtenerAsync(id);

            if (cita == null)
            {
                return NotFound();
            }

            if (cita.Estado != EstadoCita.Programada)
            {
                TempData["Error"] =
                    "La cita no se encuentra programada.";

                return RedirectToAction(nameof(Index));
            }

            await CargarDatosAsync(cita);

            return View(new RegistroAtencion());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(
            [FromRoute] int id,
            RegistroAtencion atencion)
        {
            var cita = await _atencionProxy
                .ObtenerAsync(id);

            if (cita == null)
            {
                return NotFound();
            }

            if (cita.Estado != EstadoCita.Programada)
            {
                TempData["Error"] =
                    "La cita no se encuentra programada.";

                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                await CargarDatosAsync(cita);

                return View(atencion);
            }

            var mensaje = await _atencionProxy
                .RegistrarAsync(id, atencion);

            if (mensaje != null)
            {
                ModelState.AddModelError(
                    string.Empty, mensaje);

                await CargarDatosAsync(cita);

                return View(atencion);
            }

            TempData["Exito"] =
                "Atención registrada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarDatosAsync(Cita cita)
        {
            ViewBag.Cita = cita;

            var productos = new List<Producto>();
            var pagina = 1;

            while (true)
            {
                var resultado = await _productoProxy
                    .ListarAsync(null, pagina);

                productos.AddRange(resultado.Elementos);

                if (pagina >= resultado.TotalPaginas)
                {
                    break;
                }

                pagina++;
            }

            ViewBag.Productos = productos
                .Where(p => p.Activo && p.Stock > 0)
                .OrderBy(p => p.Nombre)
                .ToList();
        }
    }
}