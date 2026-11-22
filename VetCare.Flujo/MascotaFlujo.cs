using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Interfaces;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Flujo;

public class MascotaFlujo : IMascotaFlujo
{
    private readonly IMascotaDA _da;
    private readonly IMascotaReglas _reglas;

    public MascotaFlujo(IMascotaDA da, IMascotaReglas reglas)
    {
        _da = da;
        _reglas = reglas;
    }

    public Task<List<Mascota>> ListarAsync(string? buscar, int? especieId) =>
        _da.ListarAsync(buscar, especieId);

    public async Task<Mascota?> ObtenerAsync(int id)
    {
        var mascota = await _da.ObtenerAsync(id, incluirHistorial: true);
        if (mascota is null) return null;

        // HU-09 CA-02: historial de la cita más reciente a la más antigua
        mascota.Citas = mascota.Citas.OrderByDescending(c => c.FechaHora).ToList();
        return mascota;
    }

    public Task<List<Especie>> ListarEspeciesAsync() => _da.ListarEspeciesAsync();

    public Task<List<Cliente>> ListarDuenosAsync() => _da.ListarDuenosAsync();

    public async Task<Resultado<Mascota>> RegistrarAsync(Mascota mascota)
    {
        var error = await _reglas.ValidarAsync(mascota);
        if (error is not null) return Resultado<Mascota>.Conflicto(error.Mensaje, error.Campo);

        mascota.MascotaId = 0;
        mascota.Activa = true;
        return Resultado<Mascota>.Exito(await _da.AgregarAsync(mascota));
    }

    public async Task<Resultado<Mascota>> ActualizarAsync(int id, Mascota mascota)
    {
        if (await _da.ObtenerAsync(id) is null) return Resultado<Mascota>.NoEncontrado();

        var error = await _reglas.ValidarAsync(mascota);
        if (error is not null) return Resultado<Mascota>.Conflicto(error.Mensaje, error.Campo);

        mascota.MascotaId = id;
        await _da.ActualizarAsync(mascota);
        return Resultado<Mascota>.Exito(mascota);
    }

    public async Task<Resultado<bool>> EliminarAsync(int id)
    {
        if (await _da.ObtenerAsync(id) is null) return Resultado<bool>.NoEncontrado();

        if (await _reglas.DebeDesactivarseAsync(id))
        {
            await _da.DesactivarAsync(id);       // conserva el historial
            return Resultado<bool>.Exito(true);
        }

        await _da.EliminarAsync(id);
        return Resultado<bool>.Exito(false);
    }
}