namespace SistemaCalificaciones.Models;

public class ActividadCompetencia
{
    public int IdActividadCompetencia { get; set; }

    public int IdAsignacionDocente { get; set; }
    public AsignacionDocente AsignacionDocente { get; set; } = null!;

    public int IdPeriodoPublicacion { get; set; }
    public PeriodoPublicacion PeriodoPublicacion { get; set; } = null!;

    public int IdCompetencia { get; set; }
    public Competencia Competencia { get; set; } = null!;

    public string Nombre { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public bool Activa { get; set; } = true;
}