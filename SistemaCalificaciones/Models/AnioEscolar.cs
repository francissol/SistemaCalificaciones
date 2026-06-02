using SistemaCalificaciones.Models;

public class AnioEscolar
{
    public int IdAnioEscolar { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public bool Activo { get; set; } = false;

    public bool Cerrado { get; set; } = false;

    public ICollection<Inscripcion> Inscripciones { get; set; } = new List<Inscripcion>();

    public ICollection<PeriodoPublicacion> PeriodosPublicacion { get; set; } = new List<PeriodoPublicacion>();
}