namespace SistemaCalificaciones.Models
{

    public class Observacion
    {
        public int IdObservacion { get; set; }

        public int IdEstudiante { get; set; }
        public Estudiante Estudiante { get; set; } = null!;

        public int IdAsignacionDocente { get; set; }
        public AsignacionDocente AsignacionDocente { get; set; } = null!;

        public int IdPeriodoPublicacion { get; set; }
        public PeriodoPublicacion PeriodoPublicacion { get; set; } = null!;

        public string Tipo { get; set; } = string.Empty; // ESTUDIANTE / PADRE
        public string Comentario { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}