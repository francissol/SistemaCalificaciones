using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.DTOs.Cursos;
using SistemaCalificaciones.Models;

namespace SistemaCalificaciones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class CursosController : ControllerBase
{
    private readonly AppDbContext _context;

    public CursosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var cursos = await _context.Cursos
            .Include(c => c.Grado)
            .ThenInclude(g => g.Nivel)
            .OrderBy(c => c.Grado.Orden)
            .ThenBy(c => c.Seccion)
            .Select(c => new
            {
                c.IdCurso,
                c.Nombre,
                c.Seccion,
                c.Activo,
                c.IdGrado,
                Grado = c.Grado.Nombre,
                Nivel = c.Grado.Nivel.Nombre
            })
            .ToListAsync();

        return Ok(cursos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var curso = await _context.Cursos
            .Include(c => c.Grado)
            .ThenInclude(g => g.Nivel)
            .FirstOrDefaultAsync(c => c.IdCurso == id);

        if (curso == null)
            return NotFound("Curso no encontrado.");

        return Ok(curso);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(CrearCursoDto dto)
    {
        var gradoExiste = await _context.Grados.AnyAsync(g => g.IdGrado == dto.IdGrado);

        if (!gradoExiste)
            return BadRequest("El grado seleccionado no existe.");

        var duplicado = await _context.Cursos
            .AnyAsync(c => c.Nombre == dto.Nombre && c.IdGrado == dto.IdGrado);

        if (duplicado)
            return BadRequest("Ya existe un curso con ese nombre en ese grado.");

        var curso = new Curso
        {
            IdGrado = dto.IdGrado,
            Nombre = dto.Nombre,
            Seccion = dto.Seccion,
            Activo = true
        };

        _context.Cursos.Add(curso);
        await _context.SaveChangesAsync();

        return Ok(curso);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, CrearCursoDto dto)
    {
        var curso = await _context.Cursos.FindAsync(id);

        if (curso == null)
            return NotFound("Curso no encontrado.");

        var gradoExiste = await _context.Grados.AnyAsync(g => g.IdGrado == dto.IdGrado);

        if (!gradoExiste)
            return BadRequest("El grado seleccionado no existe.");

        var duplicado = await _context.Cursos
            .AnyAsync(c => c.Nombre == dto.Nombre && c.IdGrado == dto.IdGrado && c.IdCurso != id);

        if (duplicado)
            return BadRequest("Ya existe otro curso con ese nombre en ese grado.");

        curso.IdGrado = dto.IdGrado;
        curso.Nombre = dto.Nombre;
        curso.Seccion = dto.Seccion;

        await _context.SaveChangesAsync();

        return Ok(curso);
    }

    [HttpPut("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        var curso = await _context.Cursos.FindAsync(id);

        if (curso == null)
            return NotFound("Curso no encontrado.");

        curso.Activo = !curso.Activo;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Estado del curso actualizado.",
            curso.IdCurso,
            curso.Activo
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var curso = await _context.Cursos.FindAsync(id);

        if (curso == null)
            return NotFound("Curso no encontrado.");

        var tieneInscripciones = await _context.Inscripciones.AnyAsync(i => i.IdCurso == id);

        if (tieneInscripciones)
            return BadRequest("No puedes eliminar este curso porque tiene estudiantes inscritos.");

        _context.Cursos.Remove(curso);
        await _context.SaveChangesAsync();

        return Ok("Curso eliminado correctamente.");
    }
}