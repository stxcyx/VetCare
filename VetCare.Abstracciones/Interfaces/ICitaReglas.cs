using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Abstracciones.Interfaces;

public interface ICitaReglas
{
    // RN-01 y RN-02: fecha futura, dentro del horario del veterinario y sin traslape.
    // Devuelve null si todo se cumple o el error con el campo afectado.
    Task<ErrorRespuesta?> ValidarAgendaAsync(Cita cita, int? citaId = null);

    // RN-03: solo una cita Programada se reprograma o se cancela.
    string? ValidarEstadoModificable(Cita cita);
}
