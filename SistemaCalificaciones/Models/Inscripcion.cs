namespace SistemaCalificaciones.Models
{
 

    public class Inscripcion
    {
        public int IdInscripcion { get; set; }

        public int IdEstudiante { get; set; }
        public Estudiante Estudiante { get; set; } = null!;

        public int IdCurso { get; set; }
        public Curso Curso { get; set; } = null!;

        public int IdAnioEscolar { get; set; }
        public AnioEscolar AnioEscolar { get; set; } = null!;

        public DateTime FechaInscripcion { get; set; } = DateTime.Now;
        public string Estado { get; set; } = "Activo";
    }
}