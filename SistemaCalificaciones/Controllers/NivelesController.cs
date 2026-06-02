using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.DTOs.Niveles;
using SistemaCalificaciones.Models;

namespace SistemaCalificaciones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class NivelesController : ControllerBase
{
    private readonly AppDbContext _context;

    public NivelesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var niveles = await _context.Niveles
            .OrderBy(n => n.Nombre)
            .ToListAsync();

        return Ok(niveles);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var nivel = await _context.Niveles.FindAsync(id);

        if (nivel == null)
            return NotFound("Nivel no encontrado.");

        return Ok(nivel);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(CrearNivelDto dto)
    {
        var existe = await _context.Niveles
            .AnyAsync(n => n.Nombre == dto.Nombre);

        if (existe)
            return BadRequest("Ya existe un nivel con ese nombre.");

        var nivel = new Nivel
        {
            Nombre = dto.Nombre
        };

        _context.Niveles.Add(nivel);
        await _context.SaveChangesAsync();

        return Ok(nivel);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, CrearNivelDto dto)
    {
        var nivel = await _context.Niveles.FindAsync(id);

        if (nivel == null)
            return NotFound("Nivel no encontrado.");

        var existe = await _context.Niveles
            .AnyAsync(n => n.Nombre == dto.Nombre && n.IdNivel != id);

        if (existe)
            return BadRequest("Ya existe otro nivel con ese nombre.");

        nivel.Nombre = dto.Nombre;

        await _context.SaveChangesAsync();

        return Ok(nivel);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var nivel = await _context.Niveles.FindAsync(id);

        if (nivel == null)
            return NotFound("Nivel no encontrado.");

        var tieneGrados = await _context.Grados
            .AnyAsync(g => g.IdNivel == id);

        if (tieneGrados)
            return BadRequest("No puedes eliminar este nivel porque tiene grados asociados.");

        _context.Niveles.Remove(nivel);
        await _context.SaveChangesAsync();

        return Ok("Nivel eliminado correctamente.");
    }
}