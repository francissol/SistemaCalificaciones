namespace SistemaCalificaciones.Models
{

    public class PeriodoPublicacion
    {
        public int IdPeriodoPublicacion { get; set; }

        public int IdAnioEscolar { get; set; }
        public AnioEscolar AnioEscolar { get; set; } = null!;

        public string Nombre { get; set; } = string.Empty;

        public DateTime FechaInicio { get; set; }
        public DateTime FechaCierre { get; set; }

        public bool Activo { get; set; } = true;

        public ICollection<ActividadEvaluativa> ActividadesEvaluativas { get; set; } = new List<ActividadEvaluativa>();
        public ICollection<CalificacionPeriodo> CalificacionesPeriodo { get; set; } = new List<CalificacionPeriodo>();
    }
}