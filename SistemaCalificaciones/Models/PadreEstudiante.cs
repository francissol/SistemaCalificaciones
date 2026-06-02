namespace SistemaCalificaciones.Models;


public class PadreEstudiante
{
    public int IdPadreEstudiante { get; set; }

    public int IdPadre { get; set; }
    public Padre Padre { get; set; } = null!;

    public int IdEstudiante { get; set; }
    public Estudiante Estudiante { get; set; } = null!;

    public string Parentesco { get; set; } = string.Empty;
    public bool ResponsableAcademico { get; set; } = true;
}