using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaCalificaciones.Models;

public class ActividadCompetencia
{
    public int IdActividadCompetencia { get; set; }

    public int IdAsignacionDocente { get; set; }
    public AsignacionDocente AsignacionDocente { get; set; } = null!;

    public int ? IdPeriodoPublicacion { get; set; }
    public PeriodoPublicacion ? PeriodoPublicacion { get; set; }
    public int ? IdCompetencia { get; set; }
    public Competencia?  Competencia { get; set; } = null!;

    public int? IdResultadoAprendizaje { get; set; }

    [ForeignKey("IdResultadoAprendizaje")]
    public ResultadoAprendizaje? ResultadoAprendizaje { get; set; }
    public string Nombre { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public bool Activa { get; set; } = true;
}