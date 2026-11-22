using System.ComponentModel.DataAnnotations;

namespace VetCare.Abstracciones.Modelos;

public class CitaProducto
{
    public int CitaId { get; set; }
    public Cita? Cita { get; set; }

    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que 0")]
    public int Cantidad { get; set; }
}