using System.ComponentModel.DataAnnotations;

namespace VetCare.Abstracciones.Modelos
{
    public class RegistroAtencion
    {
        [Required(ErrorMessage = "El diagnóstico es obligatorio.")]
        [StringLength(1000)]
        public string Diagnostico { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tratamiento es obligatorio.")]
        [StringLength(1000)]
        public string Tratamiento { get; set; } = string.Empty;

        public List<ProductoAtencion> Productos { get; set; } = new();
    }

    public class ProductoAtencion
    {
        [Range(1, int.MaxValue,
            ErrorMessage = "Debe seleccionar un producto.")]
        public int ProductoId { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public int Cantidad { get; set; }
    }
}