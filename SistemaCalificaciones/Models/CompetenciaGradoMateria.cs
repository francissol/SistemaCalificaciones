namespace SistemaCalificaciones.Models
{
    public class CompetenciaGradoMateria
    {
        public int IdCompetenciaGradoMateria { get; set; }

        public int IdGrado { get; set; }

        public int IdMateria { get; set; }

        public int IdCompetencia { get; set; }

        public string? Descripcion { get; set; }

        public Grado Grado { get; set; } = null!;

        public Materia Materia { get; set; } = null!;

        public Competencia Competencia { get; set; } = null!;
    }
}
