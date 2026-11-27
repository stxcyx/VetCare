using Microsoft.EntityFrameworkCore;
using VetCare.Abstracciones.Interfaces;
using VetCare.Abstracciones.Modelos;

namespace VetCare.DA.Repositorios;

public class CitaDA : ICitaDA
{
    private readonly ApplicationDbContext _context;

    public CitaDA(ApplicationDbContext context) => _context = context;

    public async Task<List<Cita>> ListarAsync(DateTime? fecha, int? veterinarioId, EstadoCita? estado)
    {
        IQueryable<Cita> consulta = _context.Citas.AsNoTracking()
            .Include(c => c.Mascota)
            .Include(c => c.Veterinario);

        if (fecha is not null)
        {
            var desde = fecha.Value.Date;
            var hasta = desde.AddDays(1);
            consulta = consulta.Where(c => c.FechaHora >= desde && c.FechaHora < hasta);
        }

        if (veterinarioId is not null)
            consulta = consulta.Where(c => c.VeterinarioId == veterinarioId);

        if (estado is not null)
            consulta = consulta.Where(c => c.Estado == estado);

        return await consulta.OrderBy(c => c.FechaHora).ToListAsync();
    }

    public Task<Cita?> ObtenerAsync(int id) =>
        _context.Citas.AsNoTracking()
            .Include(c => c.Mascota).ThenInclude(m => m!.Cliente)
            .Include(c => c.Veterinario)
            .FirstOrDefaultAsync(c => c.CitaId == id);

    public Task<List<Mascota>> ListarMascotasActivasAsync() =>
        _context.Mascotas.AsNoTracking()
            .Include(m => m.Cliente)
            .Where(m => m.Activa)
            .OrderBy(m => m.Nombre)
            .ToListAsync();

    // HU-14 CA-02: un veterinario Inactivo no aparece para agendar
    public Task<List<Veterinario>> ListarVeterinariosActivosAsync() =>
        _context.Veterinarios.AsNoTracking()
            .Where(v => v.Activo)
            .OrderBy(v => v.Nombre)
            .ToListAsync();

    public Task<Veterinario?> ObtenerVeterinarioAsync(int id) =>
        _context.Veterinarios.AsNoTracking().FirstOrDefaultAsync(v => v.VeterinarioId == id);

    public Task<bool> ExisteMascotaActivaAsync(int mascotaId) =>
        _context.Mascotas.AnyAsync(m => m.MascotaId == mascotaId && m.Activa);

    // Dos citas se traslapan si cada una empieza antes de que termine la otra.
    // Solo cuentan las Programadas: una cita cancelada libera el horario.
    public Task<bool> ExisteTraslapeAsync(int veterinarioId, DateTime inicio, DateTime fin, int? excluirCitaId = null) =>
        _context.Citas.AnyAsync(c => c.VeterinarioId == veterinarioId
                                     && c.Estado == EstadoCita.Programada
                                     && c.CitaId != excluirCitaId
                                     && c.FechaHora < fin
                                     && c.FechaHora.AddMinutes(c.DuracionMin) > inicio);

    public Task<int?> ObtenerVeterinarioIdPorUsuarioAsync(string usuarioId) =>
        _context.Veterinarios.AsNoTracking()
            .Where(v => v.UsuarioId == usuarioId)
            .Select(v => (int?)v.VeterinarioId)
            .FirstOrDefaultAsync();

    public async Task<Cita> AgregarAsync(Cita cita)
    {
        _context.Citas.Add(cita);
        await _context.SaveChangesAsync();
        return cita;
    }

    public async Task<bool> ReprogramarAsync(int id, DateTime nuevaFechaHora)
    {
        var cita = await _context.Citas.FindAsync(id);
        if (cita is null) return false;

        cita.FechaHora = nuevaFechaHora;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelarAsync(int id, string motivo)
    {
        var cita = await _context.Citas.FindAsync(id);
        if (cita is null) return false;

        cita.Estado = EstadoCita.Cancelada;
        cita.MotivoCancelacion = motivo;
        await _context.SaveChangesAsync();
        return true;
    }
}
