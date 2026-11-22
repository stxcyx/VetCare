namespace VetCare.Abstracciones.Comunes;

// Cuerpo JSON que devuelve la API cuando rechaza una operación (404 o 409).
public class ErrorRespuesta
{
    public string? Campo { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}