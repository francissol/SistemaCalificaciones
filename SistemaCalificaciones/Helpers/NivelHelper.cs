using System.Security.Claims;

namespace SistemaCalificaciones.Helpers;

public static class NivelHelper
{
    public static string? ObtenerNivelPorRol(ClaimsPrincipal user)
    {
        var rol = user.FindFirst(ClaimTypes.Role)?.Value;

        return rol switch
        {
            "CoordinadorPrimaria" => "Primaria",
            "CoordinadorSecundaria" => "Secundaria",
            "CoordinadorPolitecnico" => "Politécnico",
            _ => null
        };
    }
}