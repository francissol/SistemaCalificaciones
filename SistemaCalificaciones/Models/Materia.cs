namespace SistemaCalificaciones.Models
{

    public class Materia
    {
        public int IdMateria { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string? Abreviatura { get; set; }
        public bool EsTecnica { get; set; } = false;


        public bool Activa { get; set; } = true;

        public ICollection<GradoMateria> GradoMaterias { get; set; } = new List<GradoMateria>();
        public ICollection<AsignacionDocente> AsignacionesDocentes { get; set; } = new List<AsignacionDocente>();
    }
}
