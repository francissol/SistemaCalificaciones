namespace SistemaCalificaciones.DTOs.AniosEscolares;

public class CrearAnioEscolarDto
{
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
}