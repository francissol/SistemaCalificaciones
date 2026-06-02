namespace SistemaCalificaciones.Models
{

    public class Padre
    {
        public int IdPadre { get; set; }

        public int? IdUsuario { get; set; }
        public Usuario? Usuario { get; set; }

        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;

        public string? Cedula { get; set; }
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public string? Direccion { get; set; }

        public string? Ocupacion { get; set; }
        public string? LugarTrabajo { get; set; }
        public string? TelefonoLaboral { get; set; }

        public bool Activo { get; set; } = true;

        public ICollection<PadreEstudiante> PadreEstudiantes { get; set; } = new List<PadreEstudiante>();
    }
}