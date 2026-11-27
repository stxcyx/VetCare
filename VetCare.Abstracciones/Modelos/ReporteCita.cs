namespace VetCare.Abstracciones.Modelos;

public class ReporteCita
{
    public int CitaId { get; set; }

    public DateTime FechaHora { get; set; }

    public string Cliente { get; set; } = string.Empty;

    public string Mascota { get; set; } = string.Empty;

    public string Veterinario { get; set; } = string.Empty;

    public string Motivo { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;
}