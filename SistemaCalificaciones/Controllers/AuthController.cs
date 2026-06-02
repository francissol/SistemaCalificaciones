using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.DTOs.Auth;
using SistemaCalificaciones.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SistemaCalificaciones.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwtService;

    public AuthController(AppDbContext context, JwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.NombreUsuario == dto.NombreUsuario);

        if (usuario == null)
            return Unauthorized("Usuario o contraseña incorrectos.");

        if (!usuario.Activo)
            return Unauthorized("Este usuario está inactivo.");
        {

        }
        Console.WriteLine(usuario.PasswordHash);
        Console.WriteLine(dto.Password);

        var passwordValida = BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash);

        if (!passwordValida)
            return Unauthorized("Usuario o contraseña incorrectos.");

        usuario.UltimoAcceso = DateTime.Now;
        await _context.SaveChangesAsync();

        var token = _jwtService.GenerarToken(usuario);

        return Ok(new LoginResponseDto
        {
            Token = token,
            NombreUsuario = usuario.NombreUsuario,
            Rol = usuario.Rol.Nombre,
            DebeCambiarPassword = usuario.DebeCambiarPassword
        });
    }


    [Authorize]
    [HttpPost("cambiar-mi-password")]
    public async Task<IActionResult> CambiarMiPassword(CambiarPasswordDto dto)
    {
        var idUsuarioClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (idUsuarioClaim == null)
            return Unauthorized("Token inválido.");

        int idUsuario = int.Parse(idUsuarioClaim);

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

        if (usuario == null)
            return NotFound("Usuario no encontrado.");

        if (dto.PasswordNueva != dto.ConfirmarPassword)
            return BadRequest("Las contraseñas nuevas no coinciden.");

        if (dto.PasswordNueva.Length < 8)
            return BadRequest("La nueva contraseña debe tener al menos 8 caracteres.");

        var passwordValida = BCrypt.Net.BCrypt.Verify(dto.PasswordActual, usuario.PasswordHash);

        if (!passwordValida)
            return BadRequest("La contraseña actual no es correcta.");

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.PasswordNueva);
        usuario.DebeCambiarPassword = false;

        await _context.SaveChangesAsync();

        return Ok("Contraseña cambiada correctamente.");
    }

    [HttpGet("generar-hash")]
    public IActionResult GenerarHash()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("Admin1234");
        return Ok(hash);
    }

    [HttpPost("cambiar-password")]
    public async Task<IActionResult> CambiarPassword(CambiarPasswordDto dto)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.IdUsuario == dto.IdUsuario);

        if (usuario == null)
            return NotFound("Usuario no encontrado.");

        Console.WriteLine(usuario.PasswordHash);
        Console.WriteLine(dto.PasswordActual);

        var passwordValida = BCrypt.Net.BCrypt.Verify(dto.PasswordActual, usuario.PasswordHash);

        if (!passwordValida)
            return BadRequest("La contraseña actual no es correcta.");

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.PasswordNueva);
        usuario.DebeCambiarPassword = false;

        await _context.SaveChangesAsync();

        return Ok("Contraseña cambiada correctamente.");
    }
}