using VetCare.Abstracciones.Interfaces;

namespace VetCare.Reglas;

public class ClienteReglas : IClienteReglas
{
    public const string MsgCedulaDuplicada = "Ya existe un cliente registrado con esta cédula";
    public const string MsgTieneMascotas = "No se puede eliminar: el cliente tiene mascotas asociadas";

    private readonly IClienteDA _da;
    public ClienteReglas(IClienteDA da) => _da = da;

    // HU-04 CA-02 y HU-06 CA-02: la cédula es única
    public async Task<string?> ValidarCedulaUnicaAsync(string cedula, int? clienteId = null) =>
        await _da.ExisteCedulaAsync(cedula, clienteId) ? MsgCedulaDuplicada : null;

    // RN-06: no se elimina un cliente con mascotas
    public async Task<string?> ValidarEliminacionAsync(int clienteId) =>
        await _da.TieneMascotasAsync(clienteId) ? MsgTieneMascotas : null;
}