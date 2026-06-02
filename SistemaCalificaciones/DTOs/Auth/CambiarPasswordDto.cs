namespace SistemaCalificaciones.DTOs.Auth;

public class CambiarPasswordDto
{
    public int IdUsuario { get; set; }
    public string PasswordActual { get; set; } = string.Empty;
    public string PasswordNueva { get; set; } = string.Empty;
    
    public string ConfirmarPassword { get; set; } = string.Empty;
}