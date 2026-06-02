
namespace SistemaCalificaciones.Models;

public class ActividadEvaluativa
{
    public int IdActividadEvaluativa { get; set; }

    public int IdAsignacionDocente { get; set; }
    public AsignacionDocente AsignacionDocente { get; set; } = null!;

    public int IdPeriodoPublicacion { get; set; }
    public PeriodoPublicacion PeriodoPublicacion { get; set; } = null!;

    public string Nombre { get; set; } = string.Empty;

    public decimal Porcentaje { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public bool Activa { get; set; } = true;

    public ICollection<NotaActividad> NotasActividades { get; set; } = new List<NotaActividad>();
}