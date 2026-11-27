using System.ComponentModel.DataAnnotations;

namespace VetCare.Abstracciones.Modelos;

public class FiltroReporteCita : IValidatableObject
{
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de inicio")]
    public DateTime? FechaInicio { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Fecha de fin")]
    public DateTime? FechaFin { get; set; }

    [Display(Name = "Veterinario")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un veterinario válido")]
    public int? VeterinarioId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (FechaInicio.HasValue && FechaFin.HasValue &&
            FechaInicio.Value.Date > FechaFin.Value.Date)
        {
            yield return new ValidationResult(
                "La fecha de inicio debe ser anterior a la fecha de fin",
                new[] { nameof(FechaInicio), nameof(FechaFin) });
        }
    }
}