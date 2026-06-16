using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaCalificaciones.Models;

public class ResultadoAprendizaje
{
    [Key]
    public int IdResultadoAprendizaje { get; set; }

    [Required]
    public int IdAsignacionDocente { get; set; }

    [ForeignKey("IdAsignacionDocente")]
    public AsignacionDocente AsignacionDocente { get; set; } = null!;

    [Required]
    [MaxLength(10)]
    public string Codigo { get; set; } = "";

    [Required]
    [MaxLength(200)]
    public string Nombre { get; set; } = "";

    [Column(TypeName = "decimal(5,2)")]
    public decimal ValorMaximo { get; set; }

    public bool Activo { get; set; } = true;
}