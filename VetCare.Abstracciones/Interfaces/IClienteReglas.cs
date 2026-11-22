namespace VetCare.Abstracciones.Interfaces;

// Cada método devuelve null si la regla se cumple o el mensaje de error si no.
public interface IClienteReglas
{
    Task<string?> ValidarCedulaUnicaAsync(string cedula, int? clienteId = null);
    Task<string?> ValidarEliminacionAsync(int clienteId);
}