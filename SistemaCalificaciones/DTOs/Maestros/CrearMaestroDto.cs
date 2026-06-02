namespace SistemaCalificaciones.DTOs.Maestros;

public class CrearMaestroDto
{
    public string? CodigoEmpleado { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string? Cedula { get; set; }
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public string? Direccion { get; set; }
    public string? Especialidad { get; set; }
    public DateTime? FechaIngreso { get; set; }
}