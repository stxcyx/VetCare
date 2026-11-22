using Microsoft.EntityFrameworkCore;
using VetCare.Abstracciones.Interfaces;
using VetCare.Abstracciones.Modelos;

namespace VetCare.DA.Repositorios;

public class MascotaDA : IMascotaDA
{
    private readonly ApplicationDbContext _context;
    public MascotaDA(ApplicationDbContext context) => _context = context;

    public async Task<List<Mascota>> ListarAsync(string? buscar, int? especieId)
    {
        IQueryable<Mascota> consulta = _context.Mascotas.AsNoTracking()
            .Include(m => m.Cliente)
            .Include(m => m.Especie);

        if (especieId is not null)
            consulta = consulta.Where(m => m.EspecieId == especieId);

        // HU-09 CA-01: busca por nombre de la mascota o del dueño
        if (!string.IsNullOrWhiteSpace(buscar))
            consulta = consulta.Where(m => m.Nombre.Contains(buscar)
                                           || m.Cliente!.Nombre.Contains(buscar)
                                           || m.Cliente!.Apellidos.Contains(buscar));

        return await consulta.OrderBy(m => m.Nombre).ToListAsync();
    }

    public async Task<Mascota?> ObtenerAsync(int id, bool incluirHistorial = false)
    {
        IQueryable<Mascota> consulta = _context.Mascotas.AsNoTracking()
            .Include(m => m.Cliente)
            .Include(m => m.Especie);

        if (incluirHistorial)
            consulta = consulta.Include(m => m.Citas).ThenInclude(c => c.Veterinario);

        return await consulta.FirstOrDefaultAsync(m => m.MascotaId == id);
    }

    public Task<List<Especie>> ListarEspeciesAsync() =>
        _context.Especies.AsNoTracking().OrderBy(e => e.Nombre).ToListAsync();

    public Task<List<Cliente>> ListarDuenosAsync() =>
        _context.Clientes.AsNoTracking().OrderBy(c => c.Apellidos).ThenBy(c => c.Nombre).ToListAsync();

    public Task<bool> ExisteClienteAsync(int clienteId) =>
        _context.Clientes.AnyAsync(c => c.ClienteId == clienteId);

    public Task<bool> ExisteEspecieAsync(int especieId) =>
        _context.Especies.AnyAsync(e => e.EspecieId == especieId);

    public Task<bool> TieneCitasAsync(int mascotaId) =>
        _context.Citas.AnyAsync(c => c.MascotaId == mascotaId);

    public async Task<Mascota> AgregarAsync(Mascota mascota)
    {
        _context.Mascotas.Add(mascota);
        await _context.SaveChangesAsync();
        return mascota;
    }

    public async Task<bool> ActualizarAsync(Mascota mascota)
    {
        var existente = await _context.Mascotas.FindAsync(mascota.MascotaId);
        if (existente is null) return false;

        existente.ClienteId = mascota.ClienteId;
        existente.EspecieId = mascota.EspecieId;
        existente.Nombre = mascota.Nombre;
        existente.Raza = mascota.Raza;
        existente.Sexo = mascota.Sexo;
        existente.FechaNacimiento = mascota.FechaNacimiento;
        existente.PesoKg = mascota.PesoKg;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var mascota = await _context.Mascotas.FindAsync(id);
        if (mascota is null) return false;
        _context.Mascotas.Remove(mascota);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DesactivarAsync(int id)
    {
        var mascota = await _context.Mascotas.FindAsync(id);
        if (mascota is null) return false;
        mascota.Activa = false;
        await _context.SaveChangesAsync();
        return true;
    }
}