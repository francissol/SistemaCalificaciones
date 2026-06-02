namespace SistemaCalificaciones.DTOs.Estudiantes;

public class CrearEstudianteDto
{
    public string Matricula { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public DateTime? FechaNacimiento { get; set; }
    public string? Sexo { get; set; }
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public string? Direccion { get; set; }
    public DateTime? FechaIngreso { get; set; }

    public int IdCurso { get; set; }
    public int IdAnioEscolar { get; set; }

    public string NombrePadre { get; set; } = string.Empty;
    public string ApellidoPadre { get; set; } = string.Empty;
    public string? TelefonoPadre { get; set; }
    public string? CorreoPadre { get; set; }
    public string Parentesco { get; set; } = "Padre";
}