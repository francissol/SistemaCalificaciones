namespace SistemaCalificaciones.DTOs.Materias;

public class CrearMateriaDto
{
    public string Nombre { get; set; } = string.Empty;
    public bool EsTecnica { get; set; } = false;
    public string? Abreviatura { get; set; }
}