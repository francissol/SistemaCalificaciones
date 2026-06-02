using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.DTOs.Grados;
using SistemaCalificaciones.Models;

namespace SistemaCalificaciones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class GradosController : ControllerBase
{
    private readonly AppDbContext _context;

    public GradosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var grados = await _context.Grados
            .Include(g => g.Nivel)
            .OrderBy(g => g.Orden)
            .Select(g => new
            {
                g.IdGrado,
                g.Nombre,
                g.Orden,
                g.IdNivel,
                Nivel = g.Nivel.Nombre
            })
            .ToListAsync();

        return Ok(grados);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var grado = await _context.Grados
            .Include(g => g.Nivel)
            .FirstOrDefaultAsync(g => g.IdGrado == id);

        if (grado == null)
            return NotFound("Grado no encontrado.");

        return Ok(grado);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(CrearGradoDto dto)
    {
        var nivelExiste = await _context.Niveles.AnyAsync(n => n.IdNivel == dto.IdNivel);

        if (!nivelExiste)
            return BadRequest("El nivel seleccionado no existe.");

        var existe = await _context.Grados
            .AnyAsync(g => g.Nombre == dto.Nombre && g.IdNivel == dto.IdNivel);

        if (existe)
            return BadRequest("Ya existe ese grado en ese nivel.");

        var grado = new Grado
        {
            IdNivel = dto.IdNivel,
            Nombre = dto.Nombre,
            Orden = dto.Orden
        };

        _context.Grados.Add(grado);
        await _context.SaveChangesAsync();

        return Ok(grado);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, CrearGradoDto dto)
    {
        var grado = await _context.Grados.FindAsync(id);

        if (grado == null)
            return NotFound("Grado no encontrado.");

        var nivelExiste = await _context.Niveles.AnyAsync(n => n.IdNivel == dto.IdNivel);

        if (!nivelExiste)
            return BadRequest("El nivel seleccionado no existe.");

        var duplicado = await _context.Grados
            .AnyAsync(g => g.Nombre == dto.Nombre && g.IdNivel == dto.IdNivel && g.IdGrado != id);

        if (duplicado)
            return BadRequest("Ya existe otro grado con ese nombre en ese nivel.");

        grado.IdNivel = dto.IdNivel;
        grado.Nombre = dto.Nombre;
        grado.Orden = dto.Orden;

        await _context.SaveChangesAsync();

        return Ok(grado);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var grado = await _context.Grados.FindAsync(id);

        if (grado == null)
            return NotFound("Grado no encontrado.");

        var tieneCursos = await _context.Cursos.AnyAsync(c => c.IdGrado == id);

        if (tieneCursos)
            return BadRequest("No puedes eliminar este grado porque tiene cursos asociados.");

        _context.Grados.Remove(grado);
        await _context.SaveChangesAsync();

        return Ok("Grado eliminado correctamente.");
    }
}