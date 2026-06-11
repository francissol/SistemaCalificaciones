namespace SistemaCalificaciones.Models
{
    public class Competencia
    {
        public int IdCompetencia { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public bool Activa { get; set; } = true;


    }
}