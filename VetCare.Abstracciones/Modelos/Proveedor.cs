using System.ComponentModel.DataAnnotations;

namespace VetCare.Abstracciones.Modelos;

public class Proveedor
{
    public int ProveedorId { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio"), StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}