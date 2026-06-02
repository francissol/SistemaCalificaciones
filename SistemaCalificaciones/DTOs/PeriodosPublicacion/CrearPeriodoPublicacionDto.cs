namespace SistemaCalificaciones.DTOs.PeriodosPublicacion;

public class CrearPeriodoPublicacionDto
{
    public int IdAnioEscolar { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaCierre { get; set; }
}