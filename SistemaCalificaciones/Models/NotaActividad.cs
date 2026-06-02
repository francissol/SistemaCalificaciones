namespace SistemaCalificaciones.Models
{


    public class NotaActividad
    {
        public int IdNotaActividad { get; set; }

        public int IdActividadEvaluativa { get; set; }
        public ActividadEvaluativa ActividadEvaluativa { get; set; } = null!;

        public int IdEstudiante { get; set; }
        public Estudiante Estudiante { get; set; } = null!;

        public decimal Nota { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public DateTime? FechaUltimaModificacion { get; set; }
    }
}