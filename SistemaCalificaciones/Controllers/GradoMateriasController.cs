using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.DTOs.GradoMaterias;
using SistemaCalificaciones.Models;
using SistemaCalificaciones.Helpers;

namespace SistemaCalificaciones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,CoordinadorPrimaria,CoordinadorSecundaria,CoordinadorPolitecnico")]
public class GradoMateriasController : ControllerBase
{
    private readonly AppDbContext _context;

    public GradoMateriasController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var data = await _context.GradoMaterias
            .Include(gm => gm.Grado)
            .Include(gm => gm.Materia)
            .OrderBy(gm => gm.Grado.Orden)
            .ThenBy(gm => gm.Materia.Nombre)
            .Select(gm => new
            {
                gm.IdGradoMateria,
                gm.IdGrado,
                Grado = gm.Grado.Nombre,
                gm.IdMateria,
                Materia = gm.Materia.Nombre,
                Abreviatura = gm.Materia.Abreviatura
            })
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("grado/{idGrado}")]
    public async Task<IActionResult> GetPorGrado(int idGrado)
    {
        var materias = await _context.GradoMaterias
            .Include(gm => gm.Materia)
            .Where(gm => gm.IdGrado == idGrado)
            .OrderBy(gm => gm.Materia.Nombre)
            .Select(gm => new
            {
                gm.IdGradoMateria,
                gm.IdMateria,
                Materia = gm.Materia.Nombre,
                Abreviatura = gm.Materia.Abreviatura
            })
            .ToListAsync();

        return Ok(materias);
    }

 

    [HttpPost]
    public async Task<IActionResult> Crear(CrearGradoMateriaDto dto)
    {
        if (dto.PorSeccion)
        {
            if (dto.IdCurso == null)
                return BadRequest("Debe seleccionar una sección.");

            var curso = await _context.Cursos
                .Include(c => c.Grado)
                .FirstOrDefaultAsync(c => c.IdCurso == dto.IdCurso);

            if (curso == null)
                return BadRequest("El curso/sección no existe.");

            if (curso.IdGrado != dto.IdGrado)
                return BadRequest("La sección seleccionada no pertenece al grado.");

            var existeCursoMateria = await _context.CursoMaterias
                .AnyAsync(cm =>
                    cm.IdCurso == dto.IdCurso.Value &&
                    cm.IdMateria == dto.IdMateria);

            if (existeCursoMateria)
                return BadRequest("Esta materia ya está asignada a esa sección.");

            var cursoMateria = new CursoMateria
            {
                IdCurso = dto.IdCurso.Value,
                IdMateria = dto.IdMateria
            };

            _context.CursoMaterias.Add(cursoMateria);
            await _context.SaveChangesAsync();

            return Ok("Materia asignada correctamente a la sección.");
        }

        var existeGradoMateria = await _context.GradoMaterias
            .AnyAsync(gm =>
                gm.IdGrado == dto.IdGrado &&
                gm.IdMateria == dto.IdMateria);

        if (existeGradoMateria)
            return BadRequest("Esta materia ya está asignada a ese grado.");

        var gradoMateria = new GradoMateria
        {
            IdGrado = dto.IdGrado,
            IdMateria = dto.IdMateria
        };

        _context.GradoMaterias.Add(gradoMateria);
        await _context.SaveChangesAsync();

        return Ok("Materia asignada correctamente al grado.");
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var asignacion = await _context.GradoMaterias.FindAsync(id);

        if (asignacion == null)
            return NotFound("Asignación no encontrada.");

        _context.GradoMaterias.Remove(asignacion);
        await _context.SaveChangesAsync();

        return Ok("Materia removida del grado correctamente.");
    }
}