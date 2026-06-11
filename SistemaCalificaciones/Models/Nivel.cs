using DocumentFormat.OpenXml.Drawing;

namespace SistemaCalificaciones.Models;

public class Nivel
{
    public int IdNivel { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool UsaCompetencias { get; set; } = false;

    public ICollection<Grado> Grados { get; set; } = new List<Grado>();
} 