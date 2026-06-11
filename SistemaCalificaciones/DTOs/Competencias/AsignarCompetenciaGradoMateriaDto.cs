namespace SistemaCalificaciones.DTOs.Competencias;

   
    public class AsignarCompetenciaGradoMateriaDto
    {
        public int IdGrado { get; set; }
        public int IdMateria { get; set; }
        public int IdCompetencia { get; set; }
        public string? Descripcion { get; set; }
    }