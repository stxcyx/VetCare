using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Interfaces;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Flujo;

//Ordena cada operacion y aplica las reglas y si se cumplen, llama al DA

public class VeterinarioFlujo : IVeterinarioFlujo
{
    private readonly IVeterinarioDA _da;
    private readonly IVeterinarioReglas _reglas;

    public VeterinarioFlujo(IVeterinarioDA da, IVeterinarioReglas reglas)
    {
        _da = da;
        _reglas = reglas;
    }

    public Task<List<Veterinario>> ListarAsync(string? especialidad, bool soloActivos = false) =>
        _da.ListarAsync(especialidad, soloActivos);

    public Task<List<string>> ListarEspecialidadesAsync() => _da.ListarEspecialidadesAsync();

    public Task<List<UsuarioOpcion>> ListarUsuariosDisponiblesAsync(int? incluirVeterinarioId = null) =>
        _da.ListarUsuariosDisponiblesAsync(incluirVeterinarioId);

    public Task<Veterinario?> ObtenerAsync(int id) => _da.ObtenerAsync(id);

    public async Task<Resultado<Veterinario>> RegistrarAsync(Veterinario veterinario)
    {
        var error = await _reglas.ValidarCodigoUnicoAsync(veterinario.CodigoColegiado);
        if (error is not null)
            return Resultado<Veterinario>.Conflicto(error, nameof(Veterinario.CodigoColegiado));

        error = await _reglas.ValidarUsuarioUnicoAsync(veterinario.UsuarioId);
        if (error is not null)
            return Resultado<Veterinario>.Conflicto(error, nameof(Veterinario.UsuarioId));

        veterinario.VeterinarioId = 0;
        veterinario.Activo = true;   // HU-12 CA-01: se guarda en estado Activo
        return Resultado<Veterinario>.Exito(await _da.AgregarAsync(veterinario));
    }

    public async Task<Resultado<Veterinario>> ActualizarAsync(int id, Veterinario veterinario)
    {
        if (await _da.ObtenerAsync(id) is null) return Resultado<Veterinario>.NoEncontrado();

        var error = await _reglas.ValidarCodigoUnicoAsync(veterinario.CodigoColegiado, id);
        if (error is not null)
            return Resultado<Veterinario>.Conflicto(error, nameof(Veterinario.CodigoColegiado));

        error = await _reglas.ValidarUsuarioUnicoAsync(veterinario.UsuarioId, id);
        if (error is not null)
            return Resultado<Veterinario>.Conflicto(error, nameof(Veterinario.UsuarioId));

        veterinario.VeterinarioId = id;
        await _da.ActualizarAsync(veterinario);
        return Resultado<Veterinario>.Exito(veterinario);
    }

    public async Task<Resultado<bool>> EliminarAsync(int id)
    {
        if (await _da.ObtenerAsync(id) is null) return Resultado<bool>.NoEncontrado();

        var error = await _reglas.ValidarEliminacionAsync(id);
        if (error is not null) return Resultado<bool>.Conflicto(error);   // RN-06

        await _da.EliminarAsync(id);
        return Resultado<bool>.Exito(true);
    }
}