using Microsoft.EntityFrameworkCore;
using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Interfaces;
using VetCare.Abstracciones.Modelos;

namespace VetCare.DA.Repositorios;

public class ClienteDA : IClienteDA
{
    private readonly ApplicationDbContext _context;
    public ClienteDA(ApplicationDbContext context) => _context = context;

    public async Task<PaginaResultado<Cliente>> ListarAsync(string? buscar, int pagina, int tamano)
    {
        var consulta = _context.Clientes.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(buscar))
            consulta = consulta.Where(c => c.Nombre.Contains(buscar) || c.Apellidos.Contains(buscar)
                                           || c.Cedula.Contains(buscar));

        var total = await consulta.CountAsync();
        var elementos = await consulta.OrderBy(c => c.Apellidos).ThenBy(c => c.Nombre)
            .Skip((pagina - 1) * tamano).Take(tamano).ToListAsync();

        return new PaginaResultado<Cliente>
        {
            Elementos = elementos,
            Pagina = pagina,
            TotalRegistros = total,
            TotalPaginas = (int)Math.Ceiling(total / (double)tamano)
        };
    }

    public async Task<Cliente?> ObtenerAsync(int id, bool incluirMascotas = false)
    {
        var consulta = _context.Clientes.AsNoTracking();
        if (incluirMascotas) consulta = consulta.Include(c => c.Mascotas);
        return await consulta.FirstOrDefaultAsync(c => c.ClienteId == id);
    }

    public Task<bool> ExisteCedulaAsync(string cedula, int? excluirClienteId = null) =>
        _context.Clientes.AnyAsync(c => c.Cedula == cedula && c.ClienteId != excluirClienteId);

    public Task<bool> TieneMascotasAsync(int clienteId) =>
        _context.Mascotas.AnyAsync(m => m.ClienteId == clienteId);

    public async Task<Cliente> AgregarAsync(Cliente cliente)
    {
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return cliente;
    }

    public async Task<bool> ActualizarAsync(Cliente cliente)
    {
        var existente = await _context.Clientes.FindAsync(cliente.ClienteId);
        if (existente is null) return false;

        // Solo se copian los datos editables; UsuarioId y FechaRegistro no cambian.
        existente.Cedula = cliente.Cedula;
        existente.Nombre = cliente.Nombre;
        existente.Apellidos = cliente.Apellidos;
        existente.Telefono = cliente.Telefono;
        existente.Correo = cliente.Correo;
        existente.Direccion = cliente.Direccion;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is null) return false;
        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();
        return true;
    }
}