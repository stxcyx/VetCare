using System.ComponentModel.DataAnnotations;

namespace VetCare.Abstracciones.Modelos;

public enum CategoriaProducto
{
    Medicamento,
    Vacuna,
    Insumo,
    Alimento
}

public class Producto
{
    public int ProductoId { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio"), StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La categoría es obligatoria")]
    public CategoriaProducto Categoria { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un proveedor")]
    public int ProveedorId { get; set; }
    public Proveedor? Proveedor { get; set; }

    [Range(0, 9999999, ErrorMessage = "El precio no puede ser negativo")]
    public decimal Precio { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
    public int Stock { get; set; }

    [Display(Name = "Stock mínimo")]
    [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo")]
    public int StockMinimo { get; set; }

    [Display(Name = "Fecha de vencimiento")]
    [DataType(DataType.Date)]
    public DateTime? FechaVencimiento { get; set; }

    public bool Activo { get; set; } = true;

    public bool BajoStock => Stock <= StockMinimo;   // RN-05

    public ICollection<CitaProducto> CitaProductos { get; set; } = new List<CitaProducto>();
}