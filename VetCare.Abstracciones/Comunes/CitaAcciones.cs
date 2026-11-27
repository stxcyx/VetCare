namespace VetCare.Abstracciones.Comunes;

// Cuerpos JSON de las acciones sobre una cita existente.
public class ReprogramarCitaDto
{
    public DateTime FechaHora { get; set; }
}

public class CancelarCitaDto
{
    public string Motivo { get; set; } = string.Empty;
}
