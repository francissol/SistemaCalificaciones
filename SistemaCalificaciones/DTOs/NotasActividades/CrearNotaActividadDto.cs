namespace SistemaCalificaciones.DTOs.NotasActividades;

public class CrearNotaActividadDto
{
    public int IdActividadEvaluativa { get; set; }
    public int IdEstudiante { get; set; }
    public decimal Nota { get; set; }
}