namespace SistemaCalificaciones.Models
{


    public class GradoMateria
    {
        public int IdGradoMateria { get; set; }

        public int IdGrado { get; set; }
        public Grado Grado { get; set; } = null!;

        public int IdMateria { get; set; }
        public Materia Materia { get; set; } = null!;
    }
}