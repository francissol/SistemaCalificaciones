namespace SistemaCalificaciones.DTOs.Competencias;

public class CrearActividadCompetenciaDto
{
    public int IdAsignacionDocente { get; set; }
    public int IdPeriodoPublicacion { get; set; }
    public int IdCompetencia { get; set; }
    public string Nombre { get; set; } = string.Empty;
}