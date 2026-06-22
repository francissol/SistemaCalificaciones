using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.DTOs.Cursos;
using SistemaCalificaciones.Models;
using SistemaCalificaciones.Helpers;

namespace SistemaCalificaciones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,CoordinadorPrimaria,CoordinadorSecundaria,CoordinadorPolitecnico")]
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
        var nivelCoordinador = NivelHelper.ObtenerNivelPorRol(User);

        var query = _context.Cursos
            .Include(c => c.Grado)
                .ThenInclude(g => g.Nivel)
            .AsQueryable();

        if (nivelCoordinador != null)
        {
            query = query.Where(c => c.Grado.Nivel.Nombre == nivelCoordinador);
        }

        var cursos = await query
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
        var nivelCoordinador = NivelHelper.ObtenerNivelPorRol(User);

        var curso = await _context.Cursos
            .Include(c => c.Grado)
                .ThenInclude(g => g.Nivel)
            .FirstOrDefaultAsync(c => c.IdCurso == id);

        if (curso == null)
            return NotFound("Curso no encontrado.");

        if (nivelCoordinador != null && curso.Grado.Nivel.Nombre != nivelCoordinador)
            return Forbid();

        return Ok(curso);
    }

    [HttpGet("grado/{idGrado}")]
    public async Task<IActionResult> GetPorGrado(int idGrado)
    {
        var nivelCoordinador = NivelHelper.ObtenerNivelPorRol(User);

        var query = _context.Cursos
            .Include(c => c.Grado)
                .ThenInclude(g => g.Nivel)
            .Where(c => c.IdGrado == idGrado && c.Activo)
            .AsQueryable();

        if (nivelCoordinador != null)
        {
            query = query.Where(c => c.Grado.Nivel.Nombre == nivelCoordinador);
        }

        var cursos = await query
            .OrderBy(c => c.Seccion)
            .Select(c => new
            {
                c.IdCurso,
                c.Nombre,
                c.Seccion,
                Grado = c.Grado.Nombre,
                Nivel = c.Grado.Nivel.Nombre
            })
            .ToListAsync();

        return Ok(cursos);
    }


    [HttpPost]
    public async Task<IActionResult> Crear(CrearCursoDto dto)
    {
        var nivelCoordinador = NivelHelper.ObtenerNivelPorRol(User);

        var grado = await _context.Grados
            .Include(g => g.Nivel)
            .FirstOrDefaultAsync(g => g.IdGrado == dto.IdGrado);

        if (grado == null)
            return BadRequest("El grado seleccionado no existe.");

        if (nivelCoordinador != null && grado.Nivel.Nombre != nivelCoordinador)
            return Forbid();

        var duplicado = await _context.Cursos
            .AnyAsync(c =>
                c.Nombre == dto.Nombre &&
                c.IdGrado == dto.IdGrado &&
                c.Seccion == dto.Seccion);

        if (duplicado)
            return BadRequest("Ya existe un curso con ese nombre, grado y sección.");

        var curso = new Curso
        {
            IdGrado = dto.IdGrado,
            Nombre = dto.Nombre,
            Seccion = dto.Seccion,
            Activo = true
        };

        _context.Cursos.Add(curso);
        await _context.SaveChangesAsync();

        await AsignarCompetenciasAutomaticas(curso.IdCurso);

        return Ok(curso);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, CrearCursoDto dto)
    {
        var nivelCoordinador = NivelHelper.ObtenerNivelPorRol(User);

        var curso = await _context.Cursos
            .Include(c => c.Grado)
                .ThenInclude(g => g.Nivel)
            .FirstOrDefaultAsync(c => c.IdCurso == id);

        if (curso == null)
            return NotFound("Curso no encontrado.");

        if (nivelCoordinador != null && curso.Grado.Nivel.Nombre != nivelCoordinador)
            return Forbid();

        var grado = await _context.Grados
            .Include(g => g.Nivel)
            .FirstOrDefaultAsync(g => g.IdGrado == dto.IdGrado);

        if (grado == null)
            return BadRequest("El grado seleccionado no existe.");

        if (nivelCoordinador != null && grado.Nivel.Nombre != nivelCoordinador)
            return Forbid();

        var duplicado = await _context.Cursos
            .AnyAsync(c =>
                c.Nombre == dto.Nombre &&
                c.IdGrado == dto.IdGrado &&
                c.Seccion == dto.Seccion &&
                c.IdCurso != id);

        if (duplicado)
            return BadRequest("Ya existe otro curso con ese nombre, grado y sección.");

        curso.IdGrado = dto.IdGrado;
        curso.Nombre = dto.Nombre;
        curso.Seccion = dto.Seccion;

        await _context.SaveChangesAsync();

        return Ok(curso);
    }

    [HttpPut("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        var nivelCoordinador = NivelHelper.ObtenerNivelPorRol(User);

        var curso = await _context.Cursos
            .Include(c => c.Grado)
                .ThenInclude(g => g.Nivel)
            .FirstOrDefaultAsync(c => c.IdCurso == id);

        if (curso == null)
            return NotFound("Curso no encontrado.");

        if (nivelCoordinador != null && curso.Grado.Nivel.Nombre != nivelCoordinador)
            return Forbid();

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
        var nivelCoordinador = NivelHelper.ObtenerNivelPorRol(User);

        var curso = await _context.Cursos
            .Include(c => c.Grado)
                .ThenInclude(g => g.Nivel)
            .FirstOrDefaultAsync(c => c.IdCurso == id);

        if (curso == null)
            return NotFound("Curso no encontrado.");

        if (nivelCoordinador != null && curso.Grado.Nivel.Nombre != nivelCoordinador)
            return Forbid();

        var tieneInscripciones = await _context.Inscripciones
            .AnyAsync(i => i.IdCurso == id);

        if (tieneInscripciones)
            return BadRequest("No puedes eliminar este curso porque tiene estudiantes inscritos.");

        _context.Cursos.Remove(curso);
        await _context.SaveChangesAsync();

        return Ok("Curso eliminado correctamente.");
    }

    private async Task AsignarCompetenciasAutomaticas(int idCurso)
    {
        var curso = await _context.Cursos
            .Include(c => c.Grado)
                .ThenInclude(g => g.Nivel)
            .FirstOrDefaultAsync(c => c.IdCurso == idCurso);

        if (curso == null)
            return;

        var nivel = curso.Grado.Nivel.Nombre.ToLower();

        List<int> idsCompetencias;

        if (nivel.Contains("primaria"))
        {
            idsCompetencias = new List<int> { 1, 2, 3 };
        }
        else if (nivel.Contains("secundaria") || nivel.Contains("pol"))
        {
            idsCompetencias = new List<int> { 1, 2, 6, 7 };
        }
        else
        {
            return;
        }

        var materias = await _context.Materias
            .Where(m => m.Activa && !m.EsTecnica)
            .ToListAsync();

        foreach (var materia in materias)
        {
            foreach (var idCompetencia in idsCompetencias)
            {
                var existe = await _context.CompetenciasGradoMateria
                    .AnyAsync(cgm =>
                        cgm.IdGrado == curso.IdGrado &&
                        cgm.IdMateria == materia.IdMateria &&
                        cgm.IdCompetencia == idCompetencia);

                if (!existe)
                {
                    _context.CompetenciasGradoMateria.Add(new CompetenciaGradoMateria
                    {
                        IdGrado = curso.IdGrado,
                        IdMateria = materia.IdMateria,
                        IdCompetencia = idCompetencia
                    });
                }
            }
        }

        await _context.SaveChangesAsync();
    }
}