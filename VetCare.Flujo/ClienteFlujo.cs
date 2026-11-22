using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Interfaces;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Flujo;

// Coordina cada operación: aplica las reglas y, si se cumplen, llama a DA.
public class ClienteFlujo : IClienteFlujo
{
    private const int TamanoPagina = 10;   // HU-05 CA-01

    private readonly IClienteDA _da;
    private readonly IClienteReglas _reglas;

    public ClienteFlujo(IClienteDA da, IClienteReglas reglas)
    {
        _da = da;
        _reglas = reglas;
    }

    public Task<PaginaResultado<Cliente>> ListarAsync(string? buscar, int pagina) =>
        _da.ListarAsync(buscar, Math.Max(1, pagina), TamanoPagina);

    public Task<Cliente?> ObtenerAsync(int id) => _da.ObtenerAsync(id, incluirMascotas: true);

    public async Task<Resultado<Cliente>> RegistrarAsync(Cliente cliente)
    {
        var error = await _reglas.ValidarCedulaUnicaAsync(cliente.Cedula);
        if (error is not null) return Resultado<Cliente>.Conflicto(error, nameof(Cliente.Cedula));

        cliente.ClienteId = 0;
        cliente.FechaRegistro = DateTime.Now;
        return Resultado<Cliente>.Exito(await _da.AgregarAsync(cliente));
    }

    public async Task<Resultado<Cliente>> ActualizarAsync(int id, Cliente cliente)
    {
        if (await _da.ObtenerAsync(id) is null) return Resultado<Cliente>.NoEncontrado();

        var error = await _reglas.ValidarCedulaUnicaAsync(cliente.Cedula, id);
        if (error is not null) return Resultado<Cliente>.Conflicto(error, nameof(Cliente.Cedula));

        cliente.ClienteId = id;
        await _da.ActualizarAsync(cliente);
        return Resultado<Cliente>.Exito(cliente);
    }

    public async Task<Resultado<bool>> EliminarAsync(int id)
    {
        if (await _da.ObtenerAsync(id) is null) return Resultado<bool>.NoEncontrado();

        var error = await _reglas.ValidarEliminacionAsync(id);
        if (error is not null) return Resultado<bool>.Conflicto(error);

        await _da.EliminarAsync(id);
        return Resultado<bool>.Exito(true);
    }
}