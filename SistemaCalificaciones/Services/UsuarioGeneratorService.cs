using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;

namespace SistemaCalificaciones.Services;

public class UsuarioGeneratorService
{
    private readonly AppDbContext _context;

    public UsuarioGeneratorService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerarNombreUsuarioAsync(string nombres, string apellidos)
    {
        var primeraLetraNombre = nombres.Trim()[0].ToString().ToUpper();
        var primeraLetraApellido = apellidos.Trim()[0].ToString().ToUpper();

        var baseUsuario = primeraLetraNombre + primeraLetraApellido;

        int contador = 1;
        string nombreUsuario;

        do
        {
            nombreUsuario = $"{baseUsuario}{contador}";
            contador++;
        }
        while (await _context.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario));

        return nombreUsuario;
    }

    public string GenerarPasswordTemporal()
    {
        return "Mir1234";
    }

    public string HashearPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}