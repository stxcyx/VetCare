namespace VetCare.Abstracciones.Modelos
{
    public class ResumenReporteCitas
    {
        public int TotalCitas { get; set; }

        public int Programadas { get; set; }

        public int Atendidas { get; set; }

        public int Canceladas { get; set; }

        public List<CitasPorEstado> PorEstado { get; set; } = new();

        public List<CitasPorVeterinario> PorVeterinario { get; set; } = new();
    }
}