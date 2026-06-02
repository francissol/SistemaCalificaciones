namespace SistemaCalificaciones.DTOs.ActividadesEvaluativas;

public class CrearActividadEvaluativaDto
{
    public int IdAsignacionDocente { get; set; }
    public int IdPeriodoPublicacion { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Porcentaje { get; set; }
}