namespace SistemaCalificaciones.DTOs.Grados;

public class CrearGradoDto
{
    public int IdNivel { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Orden { get; set; }
}
