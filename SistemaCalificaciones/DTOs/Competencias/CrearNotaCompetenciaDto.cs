namespace SistemaCalificaciones.DTOs.Competencias;

public class CrearNotaCompetenciaDto
{
    public int IdActividadCompetencia { get; set; }
    public int IdEstudiante { get; set; }
    public decimal Nota { get; set; }
}