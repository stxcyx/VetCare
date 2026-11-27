using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Interfaces;
using VetCare.Abstracciones.Modelos;

namespace VetCare.DA.Repositorios;

// Acceso a cuentas y roles mediante ASP.NET Core Identity.
// Una cuenta inactiva queda bloqueada (LockoutEnd en el futuro), así Identity no la deja iniciar sesión.
public class UsuarioDA : IUsuarioDA
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _um;

    public UsuarioDA(ApplicationDbContext context, UserManager<ApplicationUser> um)
    {
        _context = context;
        _um = um;
    }

    private IQueryable<UsuarioCuenta> Consulta()
    {
        var ahora = DateTimeOffset.UtcNow;
        return from u in _context.Users
               join x in _context.UserRoles on u.Id equals x.UserId into urs
               from ur in urs.DefaultIfEmpty()
               join y in _context.Roles on ur.RoleId equals y.Id into rs
               from r in rs.DefaultIfEmpty()
               select new UsuarioCuenta
               {
                   Id = u.Id,
                   NombreCompleto = u.NombreCompleto,
                   Email = u.Email ?? string.Empty,
                   Rol = r.Name ?? string.Empty,
                   Activo = u.LockoutEnd == null || u.LockoutEnd <= ahora
               };
    }

    public async Task<List<UsuarioCuenta>> ListarAsync(string? rol)
    {
        var consulta = Consulta();
        if (!string.IsNullOrWhiteSpace(rol)) consulta = consulta.Where(c => c.Rol == rol);
        return await consulta.OrderBy(c => c.NombreCompleto).ToListAsync();
    }

    public Task<UsuarioCuenta?> ObtenerAsync(string id) =>
        Consulta().Where(c => c.Id == id).FirstOrDefaultAsync();

    public async Task<bool> ExisteCorreoAsync(string email) =>
        await _um.FindByEmailAsync(email) is not null;

    public Task<int> ContarAdministradoresActivosAsync(string? excluirUsuarioId = null) =>
        Consulta().CountAsync(c => c.Rol == RolesSistema.Administrador
                                   && c.Activo
                                   && c.Id != excluirUsuarioId);

    public Task<bool> TieneVeterinarioAsync(string usuarioId) =>
        _context.Veterinarios.AnyAsync(v => v.UsuarioId == usuarioId);

    public async Task<(string? Error, string? Id)> CrearAsync(CrearUsuarioDto dto)
    {
        var usuario = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            EmailConfirmed = true,
            NombreCompleto = dto.NombreCompleto
        };

        var creado = await _um.CreateAsync(usuario, dto.Clave);
        if (!creado.Succeeded) return (Descripcion(creado), null);

        var conRol = await _um.AddToRoleAsync(usuario, dto.Rol);
        if (!conRol.Succeeded)
        {
            await _um.DeleteAsync(usuario);   // no deja una cuenta sin rol
            return (Descripcion(conRol), null);
        }

        return (null, usuario.Id);
    }

    public async Task<string?> CambiarRolAsync(string id, string rol)
    {
        var usuario = await _um.FindByIdAsync(id);
        if (usuario is null) return "Registro no encontrado";

        var actuales = await _um.GetRolesAsync(usuario);
        if (actuales.Count > 0)
        {
            var quitado = await _um.RemoveFromRolesAsync(usuario, actuales);
            if (!quitado.Succeeded) return Descripcion(quitado);
        }

        var agregado = await _um.AddToRoleAsync(usuario, rol);
        if (!agregado.Succeeded) return Descripcion(agregado);

        await _um.UpdateSecurityStampAsync(usuario);   // la sesión abierta se revalida con el rol nuevo
        return null;
    }

    public async Task<string?> CambiarEstadoAsync(string id, bool activo)
    {
        var usuario = await _um.FindByIdAsync(id);
        if (usuario is null) return "Registro no encontrado";

        await _um.SetLockoutEnabledAsync(usuario, true);
        var resultado = await _um.SetLockoutEndDateAsync(usuario, activo ? null : DateTimeOffset.MaxValue);
        if (!resultado.Succeeded) return Descripcion(resultado);

        await _um.UpdateSecurityStampAsync(usuario);   // cierra las sesiones abiertas de una cuenta desactivada
        return null;
    }

    private static string Descripcion(IdentityResult r) =>
        string.Join(" ", r.Errors.Select(e => e.Description));
}
