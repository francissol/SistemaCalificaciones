namespace SistemaCalificaciones.Models
{

    public class CalificacionPeriodo
    {
        public int IdCalificacionPeriodo { get; set; }

        public int IdEstudiante { get; set; }
        public Estudiante Estudiante { get; set; } = null!;

        public int IdAsignacionDocente { get; set; }
        public AsignacionDocente AsignacionDocente { get; set; } = null!;

        public int IdPeriodoPublicacion { get; set; }
        public PeriodoPublicacion PeriodoPublicacion { get; set; } = null!;

        public decimal NotaFinal { get; set; }

        public bool Publicada { get; set; } = false;
        public DateTime? FechaPublicacion { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}