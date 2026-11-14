using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCare.Web.Data;
using VetCare.Web.Models;

namespace VetCare.Web.Controllers;

public class ClientesController : Controller
{
    private const string MsgCedulaDuplicada = "Ya existe un cliente registrado con esta cédula";

    private readonly ApplicationDbContext _context;
    public ClientesController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index(string? buscar, int pagina = 1)
    {
        const int tamano = 10;
        if (pagina < 1) pagina = 1;

        var consulta = _context.Clientes.AsQueryable();
        if (!string.IsNullOrWhiteSpace(buscar))
            consulta = consulta.Where(c => c.Nombre.Contains(buscar) || c.Apellidos.Contains(buscar)
                                           || c.Cedula.Contains(buscar));

        ViewBag.Buscar = buscar;
        ViewBag.Pagina = pagina;
        ViewBag.TotalPaginas = (int)Math.Ceiling(await consulta.CountAsync() / (double)tamano);

        return View(await consulta.OrderBy(c => c.Apellidos).ThenBy(c => c.Nombre)
            .Skip((pagina - 1) * tamano).Take(tamano).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var cliente = await _context.Clientes.Include(c => c.Mascotas)
            .FirstOrDefaultAsync(c => c.ClienteId == id);
        return cliente is null ? NotFound() : View(cliente);
    }

    [Authorize(Roles = "Administrador,Recepcionista")]
    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> Create(
        [Bind("Cedula,Nombre,Apellidos,Telefono,Correo,Direccion")] Cliente cliente)
    {
        cliente.UsuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        cliente.FechaRegistro = DateTime.Now;
        ModelState.Remove(nameof(Cliente.UsuarioId));
        ModelState.Remove(nameof(Cliente.Usuario));

        if (await _context.Clientes.AnyAsync(c => c.Cedula == cliente.Cedula))
            ModelState.AddModelError(nameof(Cliente.Cedula), MsgCedulaDuplicada);
        if (!ModelState.IsValid) return View(cliente);

        _context.Add(cliente);
        await _context.SaveChangesAsync();
        TempData["Exito"] = "Cliente registrado correctamente";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var cliente = await _context.Clientes.FindAsync(id);
        return cliente is null ? NotFound() : View(cliente);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> Edit(int id,
        [Bind("ClienteId,Cedula,Nombre,Apellidos,Telefono,Correo,Direccion")] Cliente cliente)
    {
        if (id != cliente.ClienteId) return NotFound();
        var existente = await _context.Clientes.FindAsync(id);
        if (existente is null) return NotFound();

        ModelState.Remove(nameof(Cliente.UsuarioId));
        ModelState.Remove(nameof(Cliente.Usuario));

        if (await _context.Clientes.AnyAsync(c => c.Cedula == cliente.Cedula && c.ClienteId != id))
            ModelState.AddModelError(nameof(Cliente.Cedula), MsgCedulaDuplicada);
        if (!ModelState.IsValid) return View(cliente);

        existente.Cedula = cliente.Cedula;
        existente.Nombre = cliente.Nombre;
        existente.Apellidos = cliente.Apellidos;
        existente.Telefono = cliente.Telefono;
        existente.Correo = cliente.Correo;
        existente.Direccion = cliente.Direccion;
        await _context.SaveChangesAsync();

        TempData["Exito"] = "Cliente actualizado correctamente";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var cliente = await _context.Clientes.FindAsync(id);
        return cliente is null ? NotFound() : View(cliente);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken, Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var cliente = await _context.Clientes.Include(c => c.Mascotas)
            .FirstOrDefaultAsync(c => c.ClienteId == id);
        if (cliente is null) return NotFound();

        if (cliente.Mascotas.Any())   // RN-06
        {
            TempData["Error"] = "No se puede eliminar: el cliente tiene mascotas asociadas";
            return RedirectToAction(nameof(Index));
        }

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();
        TempData["Exito"] = "Cliente eliminado correctamente";
        return RedirectToAction(nameof(Index));
    }
}