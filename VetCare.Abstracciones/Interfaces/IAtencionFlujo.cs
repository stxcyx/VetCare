using VetCare.Abstracciones.Modelos;

namespace VetCare.Abstracciones.Interfaces
{
    public interface IAtencionFlujo
    {
        Task<List<Cita>> ListarPendientesAsync();

        Task<List<Cita>> ListarHistorialAsync();

        Task<List<ProductoAtencionDetalle>> ListarProductosAsync(
            int citaId);

        Task<Cita?> ObtenerAsync(int citaId);

        Task<string?> RegistrarAsync(
            int citaId,
            RegistroAtencion atencion);
    }
}