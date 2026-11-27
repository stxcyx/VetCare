using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Interfaces;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Flujo;

// Ordena cada operación de usuarios: aplica las reglas y, si se cumplen, llama al DA
public class UsuarioFlujo : IUsuarioFlujo
{
    private readonly IUsuarioDA _da;
    private readonly IUsuarioReglas _reglas;

    public UsuarioFlujo(IUsuarioDA da, IUsuarioReglas reglas)
    {
        _da = da;
        _reglas = reglas;
    }

    public Task<List<UsuarioCuenta>> ListarAsync(string? rol) => _da.ListarAsync(rol);

    public Task<UsuarioCuenta?> ObtenerAsync(string id) => _da.ObtenerAsync(id);

    public async Task<Resultado<UsuarioCuenta>> CrearAsync(CrearUsuarioDto dto)
    {
        dto.Email = dto.Email.Trim();
        dto.NombreCompleto = dto.NombreCompleto.Trim();

        var error = _reglas.ValidarRol(dto.Rol);
        if (error is not null) return Resultado<UsuarioCuenta>.Conflicto(error, nameof(CrearUsuarioDto.Rol));

        error = await _reglas.ValidarCorreoUnicoAsync(dto.Email);
        if (error is not null) return Resultado<UsuarioCuenta>.Conflicto(error, nameof(CrearUsuarioDto.Email));

        var (errorCrear, id) = await _da.CrearAsync(dto);
        if (errorCrear is not null)
            return Resultado<UsuarioCuenta>.Conflicto(errorCrear, nameof(CrearUsuarioDto.Clave));

        return Resultado<UsuarioCuenta>.Exito((await _da.ObtenerAsync(id!))!);
    }

    public async Task<Resultado<bool>> CambiarRolAsync(string id, string rol)
    {
        var actual = await _da.ObtenerAsync(id);
        if (actual is null) return Resultado<bool>.NoEncontrado();

        var error = _reglas.ValidarRol(rol)
                    ?? await _reglas.ValidarCambioRolAsync(actual, rol);
        if (error is not null) return Resultado<bool>.Conflicto(error, nameof(CambiarRolDto.Rol));

        var errorDa = await _da.CambiarRolAsync(id, rol);
        return errorDa is null ? Resultado<bool>.Exito(true) : Resultado<bool>.Conflicto(errorDa);
    }

    public async Task<Resultado<bool>> CambiarEstadoAsync(string id, bool activo)
    {
        var actual = await _da.ObtenerAsync(id);
        if (actual is null) return Resultado<bool>.NoEncontrado();
        if (actual.Activo == activo) return Resultado<bool>.Exito(true);

        if (!activo)
        {
            var error = await _reglas.ValidarDesactivacionAsync(actual);
            if (error is not null) return Resultado<bool>.Conflicto(error);
        }

        var errorDa = await _da.CambiarEstadoAsync(id, activo);
        return errorDa is null ? Resultado<bool>.Exito(true) : Resultado<bool>.Conflicto(errorDa);
    }
}
