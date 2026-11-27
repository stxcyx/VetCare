using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetCare.Abstracciones.Modelos
{
    public enum EstadoCita
    {
        Programada,
        Atendida,
        Cancelada
    }

    public class Cita
    {
        public int CitaId { get; set; }

        public int MascotaId { get; set; }

        public Mascota? Mascota { get; set; }

        public int VeterinarioId { get; set; }

        public Veterinario? Veterinario { get; set; }

        public DateTime FechaHora { get; set; }

        public int DuracionMin { get; set; } = 30;

        [Required]
        [StringLength(200)]
        public string Motivo { get; set; } = string.Empty;

        public EstadoCita Estado { get; set; } =
            EstadoCita.Programada;

        [StringLength(200)]
        public string? MotivoCancelacion { get; set; }

        public string? Diagnostico { get; set; }

        public string? Tratamiento { get; set; }

        public ICollection<CitaProducto> Productos { get; set; }
            = new List<CitaProducto>();

        [NotMapped]
        public string? NombreMascota { get; set; }

        [NotMapped]
        public string? NombreVeterinario { get; set; }
    }
}