using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Interfaces;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Reglas;

public class MascotaReglas : IMascotaReglas
{
    public const string MsgFechaFutura = "La fecha de nacimiento no puede ser futura";
    public const string MsgSinDueno = "Debe seleccionar un dueño";
    public const string MsgSinEspecie = "Debe seleccionar una especie";

    private readonly IMascotaDA _da;
    public MascotaReglas(IMascotaDA da) => _da = da;

    public async Task<ErrorRespuesta?> ValidarAsync(Mascota mascota)
    {
        // HU-08 CA-02
        if (mascota.FechaNacimiento.Date > DateTime.Today)
            return new ErrorRespuesta { Campo = nameof(Mascota.FechaNacimiento), Mensaje = MsgFechaFutura };

        // HU-08 CA-03: el dueño y la especie deben existir
        if (!await _da.ExisteClienteAsync(mascota.ClienteId))
            return new ErrorRespuesta { Campo = nameof(Mascota.ClienteId), Mensaje = MsgSinDueno };

        if (!await _da.ExisteEspecieAsync(mascota.EspecieId))
            return new ErrorRespuesta { Campo = nameof(Mascota.EspecieId), Mensaje = MsgSinEspecie };

        return null;
    }

    // RN-06 y HU-11 CA-02
    public Task<bool> DebeDesactivarseAsync(int mascotaId) => _da.TieneCitasAsync(mascotaId);
}