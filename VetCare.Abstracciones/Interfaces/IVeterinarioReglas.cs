namespace VetCare.Abstracciones.Interfaces;

// Cada método devuelve null si la regla se cumple o el mensaje de error si no.
public interface IVeterinarioReglas
{
    Task<string?> ValidarCodigoUnicoAsync(string codigo, int? veterinarioId = null);
    Task<string?> ValidarUsuarioUnicoAsync(string usuarioId, int? veterinarioId = null);
    Task<string?> ValidarEliminacionAsync(int veterinarioId);
}