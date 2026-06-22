namespace SistemaCalificaciones.DTOs.GradoMaterias;

public class CrearGradoMateriaDto
{
    public int IdGrado { get; set; }
    public int IdMateria { get; set; }

    public bool PorSeccion { get; set; }
    public int? IdCurso { get; set; }
}