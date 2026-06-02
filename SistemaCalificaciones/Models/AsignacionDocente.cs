namespace SistemaCalificaciones.Models
{
   

    public class AsignacionDocente
    {
        public int IdAsignacionDocente { get; set; }

        public int IdMaestro { get; set; }
        public Maestro Maestro { get; set; } = null!;

        public int IdCurso { get; set; }
        public Curso Curso { get; set; } = null!;

        public int IdMateria { get; set; }
        public Materia Materia { get; set; } = null!;

        public int IdAnioEscolar { get; set; }
        public AnioEscolar AnioEscolar { get; set; } = null!;

        public bool Activo { get; set; } = true;
        public DateTime FechaAsignacion { get; set; } = DateTime.Now;

        public ICollection<ActividadEvaluativa> ActividadesEvaluativas { get; set; } = new List<ActividadEvaluativa>();
        public ICollection<CalificacionPeriodo> CalificacionesPeriodo { get; set; } = new List<CalificacionPeriodo>();
    }
}
