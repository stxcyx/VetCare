using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Interfaces;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Flujo;

// Coordina cada operación: aplica las reglas y, si se cumplen, llama a DA.
public class CitaFlujo : ICitaFlujo
{
    private const int DuracionCitaMin = 30;   // RN-01: cada cita dura 30 minutos

    private readonly ICitaDA _da;
    private readonly ICitaReglas _reglas;

    public CitaFlujo(ICitaDA da, ICitaReglas reglas)
    {
        _da = da;
        _reglas = reglas;
    }

    public Task<List<Cita>> ListarAsync(DateTime? fecha, int? veterinarioId, EstadoCita? estado) =>
        _da.ListarAsync(fecha, veterinarioId, estado);

    public Task<Cita?> ObtenerAsync(int id) => _da.ObtenerAsync(id);

    public Task<List<Mascota>> ListarMascotasActivasAsync() => _da.ListarMascotasActivasAsync();

    public Task<List<Veterinario>> ListarVeterinariosActivosAsync() => _da.ListarVeterinariosActivosAsync();

    public Task<int?> ObtenerVeterinarioIdPorUsuarioAsync(string usuarioId) =>
        _da.ObtenerVeterinarioIdPorUsuarioAsync(usuarioId);

    // HU-16
    public async Task<Resultado<Cita>> AgendarAsync(Cita cita)
    {
        cita.CitaId = 0;
        cita.DuracionMin = DuracionCitaMin;
        cita.Estado = EstadoCita.Programada;   // CA-01
        cita.MotivoCancelacion = null;
        cita.Diagnostico = null;
        cita.Tratamiento = null;
        cita.Productos = new List<CitaProducto>();

        var error = await _reglas.ValidarAgendaAsync(cita);
        if (error is not null) return Resultado<Cita>.Conflicto(error.Mensaje, error.Campo);

        return Resultado<Cita>.Exito(await _da.AgregarAsync(cita));
    }

    // HU-18 CA-01: se aplican las mismas reglas de HU-16
    public async Task<Resultado<Cita>> ReprogramarAsync(int id, DateTime nuevaFechaHora)
    {
        var cita = await _da.ObtenerAsync(id);
        if (cita is null) return Resultado<Cita>.NoEncontrado();

        var errorEstado = _reglas.ValidarEstadoModificable(cita);
        if (errorEstado is not null) return Resultado<Cita>.Conflicto(errorEstado);

        var propuesta = new Cita
        {
            CitaId = id,
            MascotaId = cita.MascotaId,
            VeterinarioId = cita.VeterinarioId,
            FechaHora = nuevaFechaHora,
            DuracionMin = cita.DuracionMin
        };

        var error = await _reglas.ValidarAgendaAsync(propuesta, id);
        if (error is not null) return Resultado<Cita>.Conflicto(error.Mensaje, error.Campo);

        await _da.ReprogramarAsync(id, nuevaFechaHora);
        cita.FechaHora = nuevaFechaHora;
        return Resultado<Cita>.Exito(cita);
    }

    // HU-18 CA-02: se cancela indicando el motivo y la cita sigue en el historial
    public async Task<Resultado<bool>> CancelarAsync(int id, string motivo)
    {
        var cita = await _da.ObtenerAsync(id);
        if (cita is null) return Resultado<bool>.NoEncontrado();

        var errorEstado = _reglas.ValidarEstadoModificable(cita);
        if (errorEstado is not null) return Resultado<bool>.Conflicto(errorEstado);

        motivo = (motivo ?? string.Empty).Trim();
        if (motivo.Length == 0)
            return Resultado<bool>.Conflicto("El motivo de cancelación es obligatorio", nameof(Cita.MotivoCancelacion));
        if (motivo.Length > 200)
            return Resultado<bool>.Conflicto("El motivo no puede superar los 200 caracteres", nameof(Cita.MotivoCancelacion));

        await _da.CancelarAsync(id, motivo);
        return Resultado<bool>.Exito(true);
    }

    // HU-26: bloques libres de 30 minutos dentro del horario del veterinario
    public async Task<Resultado<List<BloqueHorario>>> ObtenerDisponibilidadAsync(int veterinarioId, DateOnly fecha)
    {
        var veterinario = await _da.ObtenerVeterinarioAsync(veterinarioId);
        if (veterinario is null)
            return Resultado<List<BloqueHorario>>.NoEncontrado("Veterinario no encontrado");
        if (!veterinario.Activo)
            return Resultado<List<BloqueHorario>>.Conflicto("El veterinario no está activo");

        var dia = fecha.ToDateTime(TimeOnly.MinValue);
        var ocupadas = await _da.ListarAsync(dia, veterinarioId, EstadoCita.Programada);
        var ahora = DateTime.Now;

        var libres = new List<BloqueHorario>();
        var inicio = dia + veterinario.HoraInicio;
        var finJornada = dia + veterinario.HoraFin;

        for (; inicio.AddMinutes(DuracionCitaMin) <= finJornada; inicio = inicio.AddMinutes(DuracionCitaMin))
        {
            var fin = inicio.AddMinutes(DuracionCitaMin);
            if (inicio <= ahora) continue;   // RN-02: solo horas futuras
            if (ocupadas.Any(c => c.FechaHora < fin && c.FechaHora.AddMinutes(c.DuracionMin) > inicio)) continue;   // RN-01

            libres.Add(new BloqueHorario { Inicio = inicio.ToString("HH:mm"), Fin = fin.ToString("HH:mm") });
        }

        return Resultado<List<BloqueHorario>>.Exito(libres);
    }
}
