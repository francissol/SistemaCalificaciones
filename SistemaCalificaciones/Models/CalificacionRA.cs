namespace SistemaCalificaciones.Models;

public class CalificacionRA
{
    public int IdCalificacionRA { get; set; }

    public int IdAsignacionDocente { get; set; }
    public AsignacionDocente AsignacionDocente { get; set; } = null!;

    public int IdEstudiante { get; set; }
    public Estudiante Estudiante { get; set; } = null!;

    public decimal NotaFinal { get; set; }

    public bool Publicada { get; set; } = false;

    public DateTime FechaCalculo { get; set; } = DateTime.Now;
    public DateTime? FechaPublicacion { get; set; }
}