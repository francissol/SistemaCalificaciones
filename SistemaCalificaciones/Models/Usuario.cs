namespace SistemaCalificaciones.Models;

public class Usuario
{
    public int IdUsuario { get; set; }

    public int IdRol { get; set; }
    public Rol Rol { get; set; } = null!;

    public string NombreUsuario { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public bool DebeCambiarPassword { get; set; } = true;
    public bool Activo { get; set; } = true;

    public DateTime? UltimoAcceso { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public Estudiante? Estudiante { get; set; }
    public Maestro? Maestro { get; set; }
    public Padre? Padre { get; set; }
}