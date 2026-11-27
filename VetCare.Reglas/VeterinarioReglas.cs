using VetCare.Abstracciones.Interfaces;

namespace VetCare.Reglas;

public class VeterinarioReglas : IVeterinarioReglas
{
    public const string MsgCodigoDuplicado = "Ya existe un veterinario con este código";
    public const string MsgUsuarioDuplicado = "Esa cuenta de usuario ya está asignada a otro veterinario";
    public const string MsgTieneCitas = "No se puede elimiar: el veterinario tiene citas programadas";

    private readonly IVeterinarioDA _da;

    //HU-12 CA-02: el código de colegiado es único

    public async Task<string?> ValidarCodigoUnicoAsync(string codigo, int? veterinarioId = null) =>
        await _da.ExisteCodigoAsync(codigo, veterinarioId) ? MsgCodigoDuplicado : null;

    //Una cuenta de usuario por Vet

    public async Task<String?> ValidarUsuarioUnicoAsync(string usuarioId, int ? veterinarioId = null) =>
        await _da.ExisteUsuarioAsync(usuarioId, veterinarioId) ? MsgUsuarioDuplicado : null;

    // RN-06 y HU-15 CA-02: no se elimina un veterinario con citas programadas

    public async Task<string?> ValidarEliminacionAsync(int veterinarioId) =>
        await _da.TieneCitasProgramadasAsync(veterinarioId) ? MsgTieneCitas : null;




}
