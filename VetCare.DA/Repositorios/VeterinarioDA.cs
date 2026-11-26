using Microsoft.EntityFrameworkCore;
using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Interfaces;
using VetCare.Abstracciones.Modelos;

namespace VetCare.DA.Repositorios;

public class VeterinarioDA : IVeterinarioDA
{
    private readonly ApplicationDbContext _context;

    public VeterinarioDA(ApplicationDbContext context) => _context = context;

    public async Task<List<Veterinario>> ListarAsync(string? especialidad, bool soloActivos = false)
    {
        var consulta = _context.Veterinarios.AsNoTracking();

        // HU-13 CA-01: filtro por especialidad
        if (!string.IsNullOrWhiteSpace(especialidad))
            consulta = consulta.Where(v => v.Especialidad == especialidad);

        // HU-14 CA-02: Citas solo ve a los activos
        if (soloActivos)
            consulta = consulta.Where(v => v.Activo);

        return await consulta.OrderBy(v => v.Nombre).ToListAsync();
    }

    public Task<List<string>> ListarEspecialidadesAsync() =>
        _context.Veterinarios.AsNoTracking()
            .Select(v => v.Especialidad)
            .Distinct()
            .OrderBy(e => e)
            .ToListAsync();

    public Task<Veterinario?> ObtenerAsync(int id) =>
        _context.Veterinarios.AsNoTracking()
            .FirstOrDefaultAsync(v => v.VeterinarioId == id);

    public Task<bool> ExisteCodigoAsync(string codigo, int? excluirVeterinarioId = null) =>
        _context.Veterinarios.AnyAsync(v => v.CodigoColegiado == codigo
                                            && v.VeterinarioId != excluirVeterinarioId);

    public Task<bool> ExisteUsuarioAsync(string usuarioId, int? excluirVeterinarioId = null) =>
        _context.Veterinarios.AnyAsync(v => v.UsuarioId == usuarioId
                                            && v.VeterinarioId != excluirVeterinarioId);

    // RN-06 y HU-15 CA-02: solo cuentan las citas en estado Programada
    public Task<bool> TieneCitasProgramadasAsync(int veterinarioId) =>
        _context.Citas.AnyAsync(c => c.VeterinarioId == veterinarioId
                                     && c.Estado == EstadoCita.Programada);

    // Cuentas con rol Veterinario que aún no están asignadas a otro veterinario.
    // al editar, se incluye la cuenta del veterinario actual para que siga apareciendo.
    public async Task<List<UsuarioOpcion>> ListarUsuariosDisponiblesAsync(int? incluirVeterinarioId = null)
    {
        var rolId = await _context.Roles
            .Where(r => r.Name == "Veterinario")
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        if (rolId is null) return new List<UsuarioOpcion>();

        var usados = _context.Veterinarios
            .Where(v => v.VeterinarioId != incluirVeterinarioId)
            .Select(v => v.UsuarioId);

        return await _context.UserRoles
            .Where(ur => ur.RoleId == rolId)
            .Join(_context.Users, ur => ur.UserId, u => u.Id, (ur, u) => u)
            .Where(u => !usados.Contains(u.Id))
            .OrderBy(u => u.NombreCompleto)
            .Select(u => new UsuarioOpcion
            {
                Id = u.Id,
                Texto = u.NombreCompleto + " (" + u.Email + ")"
            })
            .ToListAsync();
    }

    public async Task<Veterinario> AgregarAsync(Veterinario veterinario)
    {
        _context.Veterinarios.Add(veterinario);
        await _context.SaveChangesAsync();
        return veterinario;
    }

    public async Task<bool> ActualizarAsync(Veterinario veterinario)
    {
        var existente = await _context.Veterinarios.FindAsync(veterinario.VeterinarioId);
        if (existente is null) return false;

        // Se copian solo los datos editables (HU-14: horario, especialidad, estado)
        existente.UsuarioId = veterinario.UsuarioId;
        existente.CodigoColegiado = veterinario.CodigoColegiado;
        existente.Nombre = veterinario.Nombre;
        existente.Especialidad = veterinario.Especialidad;
        existente.Telefono = veterinario.Telefono;
        existente.HoraInicio = veterinario.HoraInicio;
        existente.HoraFin = veterinario.HoraFin;
        existente.Activo = veterinario.Activo;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var veterinario = await _context.Veterinarios.FindAsync(id);
        if (veterinario is null) return false;

        _context.Veterinarios.Remove(veterinario);
        await _context.SaveChangesAsync();
        return true;
    }
}