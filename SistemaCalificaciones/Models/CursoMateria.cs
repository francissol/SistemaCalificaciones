using System.ComponentModel.DataAnnotations;

namespace SistemaCalificaciones.Models;

public class CursoMateria
{
    [Key]
    public int IdCursoMateria { get; set; }

    public int IdCurso { get; set; }
    public Curso Curso { get; set; } = null!;

    public int IdMateria { get; set; }
    public Materia Materia { get; set; } = null!;
}