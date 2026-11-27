using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Modelos;
using VetCare.Web.Proxies;

namespace VetCare.Web.Controllers;

[Authorize(Roles = "Administrador")]
public class ProductosController : Controller
{
    private readonly IProductoProxy _api;
    public ProductosController(IProductoProxy api) => _api = api;

    public async Task<IActionResult> Index(
        [FromQuery, StringLength(100)] string? buscar,
        [FromQuery, Range(1, int.MaxValue / 10)] int pagina = 1,
        [FromQuery, EnumDataType(typeof(CategoriaProducto))] CategoriaProducto? categoria = null,
        [FromQuery, RegularExpression("^(Disponible|StockBajo|Inactivo)$")] string? estado = null,
        [FromQuery, RegularExpression("^(Nombre|Categoria|Stock)$")] string ordenarPor = "Nombre",
        [FromQuery, RegularExpression("^(ASC|DESC)$")] string direccion = "ASC")
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var resultado = await _api.ListarAsync(
            buscar,
            pagina,
            categoria,
            estado,
            ordenarPor,
            direccion);

        var resumen = await _api.ObtenerResumenAsync();

        ViewBag.Buscar = buscar;
        ViewBag.Pagina = resultado.Pagina;
        ViewBag.TotalPaginas = resultado.TotalPaginas;
        ViewBag.Categoria = categoria;
        ViewBag.Estado = estado;
        ViewBag.OrdenarPor = ordenarPor;
        ViewBag.Direccion = direccion;
        ViewBag.Resumen = resumen;

        return View(resultado.Elementos);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var producto = await _api.ObtenerAsync(id.Value);
        if (producto is null) return NotFound();

        return View(producto);
    }

    public async Task<IActionResult> Create()
    {
        await CargarProveedoresAsync();
        return View(new Producto());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Nombre,Categoria,ProveedorId,Precio,Stock,StockMinimo,FechaVencimiento,Activo")] Producto producto)
    {
        if (!ModelState.IsValid)
        {
            await CargarProveedoresAsync(producto.ProveedorId);
            return View(producto);
        }

        var resultado = await _api.RegistrarAsync(producto);

        if (!resultado.EsExito)
        {
            ModelState.AddModelError(
                resultado.Campo ?? string.Empty,
                resultado.Mensaje ?? "Error");

            await CargarProveedoresAsync(producto.ProveedorId);
            return View(producto);
        }

        TempData["Exito"] = "Producto registrado correctamente";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var producto = await _api.ObtenerAsync(id.Value);
        if (producto is null) return NotFound();

        await CargarProveedoresAsync(producto.ProveedorId);

        return View(producto);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("ProductoId,Nombre,Categoria,ProveedorId,Precio,Stock,StockMinimo,FechaVencimiento,Activo")] Producto producto)
    {
        if (id != producto.ProductoId) return NotFound();

        if (!ModelState.IsValid)
        {
            await CargarProveedoresAsync(producto.ProveedorId);
            return View(producto);
        }

        var resultado = await _api.ActualizarAsync(id, producto);

        if (resultado.Tipo == TipoResultado.NoEncontrado)
            return NotFound();

        if (!resultado.EsExito)
        {
            ModelState.AddModelError(
                resultado.Campo ?? string.Empty,
                resultado.Mensaje ?? "Error");

            await CargarProveedoresAsync(producto.ProveedorId);
            return View(producto);
        }

        TempData["Exito"] = "Producto actualizado correctamente";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var producto = await _api.ObtenerAsync(id.Value);
        if (producto is null) return NotFound();

        return View(producto);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var resultado = await _api.EliminarAsync(id);

        if (resultado.Tipo == TipoResultado.NoEncontrado)
            return NotFound();

        if (!resultado.EsExito)
        {
            TempData["Error"] = resultado.Mensaje;
            return RedirectToAction(nameof(Index));
        }

        TempData["Exito"] =
            "Producto eliminado o desactivado correctamente, según su historial de uso.";

        return RedirectToAction(nameof(Index));
    }

    private async Task CargarProveedoresAsync(int? proveedorId = null)
    {
        var proveedores = await _api.ListarProveedoresAsync();

        ViewBag.Proveedores = new SelectList(
            proveedores,
            nameof(Proveedor.ProveedorId),
            nameof(Proveedor.Nombre),
            proveedorId);
    }
}