using System.ComponentModel.DataAnnotations;


namespace VetCare.Abstracciones.Modelos;

public class Veterinario : IValidatableObject
{
    public int VeterinarioId { get; set; }

    [Required(ErrorMessage = "Debe seleccionar una cuenta de usuario")]
    public string UsuarioId { get; set; } = string.Empty;


    [Required(ErrorMessage = "El código de colegiado es obligatorio"), StringLength(20)]
    public string CodigoColegiado { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio"), StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La especialidad es obligatoria"), StringLength(60)]
    public string Especialidad { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "El teléfono debe tener 8 dígitos")]
    public string Telefono { get; set; } = string.Empty;

    [DataType(DataType.Time)]
    public TimeSpan HoraInicio { get; set; }

    [DataType(DataType.Time)]
    public TimeSpan HoraFin { get; set; }

    public bool Activo { get; set; } = true;

    public ICollection<Cita> Citas { get; set; } = new List<Cita>();

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (HoraFin <= HoraInicio)
            yield return new ValidationResult("La hora de fin debe ser posterior a la de inicio",
                new[] { nameof(HoraFin) });
    }
}