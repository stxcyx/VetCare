using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Interfaces;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Reglas;

public class CitaReglas : ICitaReglas
{
    public const string MsgSinMascota = "Debe seleccionar una mascota";
    public const string MsgSinVeterinario = "Debe seleccionar un veterinario";
    public const string MsgFechaPasada = "La cita debe ser en una fecha y hora futuras";
    public const string MsgFueraHorario = "La hora está fuera del horario del veterinario";
    public const string MsgTraslape = "El veterinario ya tiene una cita en ese horario";
    public const string MsgNoModificable = "La cita no se puede modificar en su estado actual";

    private readonly ICitaDA _da;

    public CitaReglas(ICitaDA da) => _da = da;

    // HU-16 CA-02 a CA-04 (RN-01 y RN-02), en este orden
    public async Task<ErrorRespuesta?> ValidarAgendaAsync(Cita cita, int? citaId = null)
    {
        if (!await _da.ExisteMascotaActivaAsync(cita.MascotaId))
            return Error(nameof(Cita.MascotaId), MsgSinMascota);

        var veterinario = await _da.ObtenerVeterinarioAsync(cita.VeterinarioId);
        if (veterinario is null || !veterinario.Activo)
            return Error(nameof(Cita.VeterinarioId), MsgSinVeterinario);

        // CA-02: fecha y hora futuras
        if (cita.FechaHora <= DateTime.Now)
            return Error(nameof(Cita.FechaHora), MsgFechaPasada);

        // CA-04: la cita completa debe caber dentro del horario de atención
        var inicio = cita.FechaHora.TimeOfDay;
        var fin = inicio + TimeSpan.FromMinutes(cita.DuracionMin);
        if (inicio < veterinario.HoraInicio || fin > veterinario.HoraFin)
            return Error(nameof(Cita.FechaHora), MsgFueraHorario);

        // CA-03: sin traslape con otra cita del mismo veterinario
        var finCita = cita.FechaHora.AddMinutes(cita.DuracionMin);
        if (await _da.ExisteTraslapeAsync(cita.VeterinarioId, cita.FechaHora, finCita, citaId))
            return Error(nameof(Cita.FechaHora), MsgTraslape);

        return null;
    }

    // RN-03 y HU-18 CA-03
    public string? ValidarEstadoModificable(Cita cita) =>
        cita.Estado == EstadoCita.Programada ? null : MsgNoModificable;

    private static ErrorRespuesta Error(string campo, string mensaje) =>
        new() { Campo = campo, Mensaje = mensaje };
}
