using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Abstracciones.Interfaces;

public interface IMascotaReglas
{
    // Devuelve null si los datos cumplen las reglas, o el campo y el mensaje del error.
    Task<ErrorRespuesta?> ValidarAsync(Mascota mascota);

    // RN-06: una mascota con citas no se borra, se desactiva.
    Task<bool> DebeDesactivarseAsync(int mascotaId);
}