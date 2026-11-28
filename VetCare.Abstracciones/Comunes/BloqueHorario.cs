namespace VetCare.Abstracciones.Comunes;

// Bloque libre de 30 minutos en la agenda de un veterinario (HU-26)
public class BloqueHorario
{
    public string Inicio { get; set; } = string.Empty;   // HH:mm
    public string Fin { get; set; } = string.Empty;      // HH:mm
}
