using System.ComponentModel.DataAnnotations;

namespace VetCare.Web.Models;

public class Mascota : IValidatableObject
{
    public int MascotaId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un dueño")]
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una especie")]
    public int EspecieId { get; set; }
    public Especie? Especie { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio"), StringLength(60)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La raza es obligatoria"), StringLength(60)]
    public string Raza { get; set; } = string.Empty;

    [Required(ErrorMessage = "El sexo es obligatorio")]
    public string Sexo { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime FechaNacimiento { get; set; }

    [Range(0.1, 150, ErrorMessage = "El peso debe estar entre 0,1 y 150 kg")]
    public decimal PesoKg { get; set; }

    public bool Activa { get; set; } = true;

    public ICollection<Cita> Citas { get; set; } = new List<Cita>();

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (FechaNacimiento.Date > DateTime.Today)
            yield return new ValidationResult("La fecha de nacimiento no puede ser futura",
                new[] { nameof(FechaNacimiento) });
    }
}
