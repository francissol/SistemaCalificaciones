namespace SistemaCalificaciones.Models
{
    public class NotaCompetencia
    {
        public int IdNotaCompetencia { get; set; }

        public int IdActividadCompetencia { get; set; }

        public int IdEstudiante { get; set; }
        public ActividadCompetencia ActividadCompetencia { get; set; } = null!;
        public Estudiante Estudiante { get; set; } = null!;

        public decimal Nota { get; set; }

        public DateTime FechaRegistro { get; set; }
    }
}
