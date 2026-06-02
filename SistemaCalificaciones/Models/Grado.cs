using DocumentFormat.OpenXml.Vml;

namespace SistemaCalificaciones.Models
{

    public class Grado
    {
        public int IdGrado { get; set; }

        public int IdNivel { get; set; }
        public Nivel Nivel { get; set; } = null!;

        public string Nombre { get; set; } = string.Empty;
        public int Orden { get; set; }

        public ICollection<Curso> Cursos { get; set; } = new List<Curso>();
        public ICollection<GradoMateria> GradoMaterias { get; set; } = new List<GradoMateria>();
    }
}