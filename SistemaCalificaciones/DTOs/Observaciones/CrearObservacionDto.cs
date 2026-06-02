namespace SistemaCalificaciones.DTOs.Observaciones;

public class CrearObservacionDto
{
    public int IdEstudiante { get; set; }
    public int IdAsignacionDocente { get; set; }
    public int IdPeriodoPublicacion { get; set; }
    public string Tipo { get; set; } = string.Empty; // ESTUDIANTE o PADRE
    public string Comentario { get; set; } = string.Empty;
}