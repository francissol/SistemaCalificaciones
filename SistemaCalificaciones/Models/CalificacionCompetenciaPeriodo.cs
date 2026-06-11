namespace SistemaCalificaciones.Models;

public class CalificacionCompetenciaPeriodo
{
    public int IdCalificacionCompetenciaPeriodo { get; set; }

    public int IdEstudiante { get; set; }
    public Estudiante Estudiante { get; set; } = null!;

    public int IdAsignacionDocente { get; set; }
    public AsignacionDocente AsignacionDocente { get; set; } = null!;

    public int IdPeriodoPublicacion { get; set; }
    public PeriodoPublicacion PeriodoPublicacion { get; set; } = null!;

    public int IdCompetencia { get; set; }
    public Competencia Competencia { get; set; } = null!;

    public decimal Promedio { get; set; }

    public bool Publicada { get; set; } = false;
    public DateTime? FechaPublicacion { get; set; }

    public DateTime FechaRegistro { get; set; } = DateTime.Now;
}