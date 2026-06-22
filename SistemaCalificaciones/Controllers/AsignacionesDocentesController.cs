using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.DTOs.AsignacionesDocentes;
using SistemaCalificaciones.Models;

namespace SistemaCalificaciones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,CoordinadorPrimaria,CoordinadorSecundaria,CoordinadorPolitecnico")]
public class AsignacionesDocentesController : ControllerBase
{
    private readonly AppDbContext _context;

    public AsignacionesDocentesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var asignaciones = await _context.AsignacionesDocentes
            .Include(a => a.Maestro)
            .Include(a => a.Curso)
            .ThenInclude(c => c.Grado)
            .Include(a => a.Materia)
            .Include(a => a.AnioEscolar)
            .OrderBy(a => a.Curso.Nombre)
            .ThenBy(a => a.Materia.Nombre)
            .Select(a => new
            {
                a.IdAsignacionDocente,
                a.IdMaestro,
                Maestro = a.Maestro.Nombres + " " + a.Maestro.Apellidos,
                a.IdCurso,
                Curso = a.Curso.Nombre,
                Grado = a.Curso.Grado.Nombre,
                a.IdMateria,
                Materia = a.Materia.Nombre,
                a.IdAnioEscolar,
                AnioEscolar = a.AnioEscolar.Nombre,
                a.Activo,
                a.FechaAsignacion
            })
            .ToListAsync();

        return Ok(asignaciones);
    }

    [HttpGet("maestro/{idMaestro}")]
    public async Task<IActionResult> GetPorMaestro(int idMaestro)
    {
        var asignaciones = await _context.AsignacionesDocentes
            .Include(a => a.Curso)
            .ThenInclude(c => c.Grado)
            .Include(a => a.Materia)
            .Include(a => a.AnioEscolar)
            .Where(a => a.IdMaestro == idMaestro && a.Activo)
            .Select(a => new
            {
                a.IdAsignacionDocente,
                Curso = a.Curso.Nombre,
                Grado = a.Curso.Grado.Nombre,
                Materia = a.Materia.Nombre,
                AnioEscolar = a.AnioEscolar.Nombre
            })
            .ToListAsync();

        return Ok(asignaciones);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(CrearAsignacionDocenteDto dto)
    {
        var maestro = await _context.Maestros.FindAsync(dto.IdMaestro);
        if (maestro == null || !maestro.Activo)
            return BadRequest("El maestro no existe o está inactivo.");

        var curso = await _context.Cursos
            .Include(c => c.Grado)
            .FirstOrDefaultAsync(c => c.IdCurso == dto.IdCurso);

        if (curso == null || !curso.Activo)
            return BadRequest("El curso no existe o está inactivo.");

        var materia = await _context.Materias.FindAsync(dto.IdMateria);
        if (materia == null || !materia.Activa)
            return BadRequest("La materia no existe o está inactiva.");

        var anio = await _context.AniosEscolares.FindAsync(dto.IdAnioEscolar);
        if (anio == null || anio.Cerrado)
            return BadRequest("El año escolar no existe o está cerrado.");

        var materiaPerteneceAlGrado = await _context.GradoMaterias
            .AnyAsync(gm => gm.IdGrado == curso.IdGrado && gm.IdMateria == dto.IdMateria);

        if (!materiaPerteneceAlGrado)
            return BadRequest("Esta materia no pertenece al grado de ese curso.");

        var yaAsignada = await _context.AsignacionesDocentes
            .AnyAsync(a =>
                a.IdCurso == dto.IdCurso &&
                a.IdMateria == dto.IdMateria &&
                a.IdAnioEscolar == dto.IdAnioEscolar &&
                a.Activo);

        if (yaAsignada)
            return BadRequest("Ese curso ya tiene un maestro asignado para esa materia en ese año escolar.");

        var asignacion = new AsignacionDocente
        {
            IdMaestro = dto.IdMaestro,
            IdCurso = dto.IdCurso,
            IdMateria = dto.IdMateria,
            IdAnioEscolar = dto.IdAnioEscolar,
            Activo = true,
            FechaAsignacion = DateTime.Now
        };

        _context.AsignacionesDocentes.Add(asignacion);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Asignación docente creada correctamente.",
            asignacion.IdAsignacionDocente
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, CrearAsignacionDocenteDto dto)
    {
        var asignacion = await _context.AsignacionesDocentes.FindAsync(id);

        if (asignacion == null)
            return NotFound("Asignación no encontrada.");

        var curso = await _context.Cursos
            .Include(c => c.Grado)
            .FirstOrDefaultAsync(c => c.IdCurso == dto.IdCurso);

        if (curso == null)
            return BadRequest("El curso no existe.");

        var materiaPerteneceAlGrado = await _context.GradoMaterias
            .AnyAsync(gm => gm.IdGrado == curso.IdGrado && gm.IdMateria == dto.IdMateria);

        if (!materiaPerteneceAlGrado)
            return BadRequest("Esta materia no pertenece al grado de ese curso.");

        var duplicado = await _context.AsignacionesDocentes
            .AnyAsync(a =>
                a.IdCurso == dto.IdCurso &&
                a.IdMateria == dto.IdMateria &&
                a.IdAnioEscolar == dto.IdAnioEscolar &&
                a.IdAsignacionDocente != id &&
                a.Activo);

        if (duplicado)
            return BadRequest("Ese curso ya tiene un maestro asignado para esa materia en ese año escolar.");

        asignacion.IdMaestro = dto.IdMaestro;
        asignacion.IdCurso = dto.IdCurso;
        asignacion.IdMateria = dto.IdMateria;
        asignacion.IdAnioEscolar = dto.IdAnioEscolar;

        await _context.SaveChangesAsync();

        return Ok("Asignación actualizada correctamente.");
    }

    [HttpPut("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        var asignacion = await _context.AsignacionesDocentes.FindAsync(id);

        if (asignacion == null)
            return NotFound("Asignación no encontrada.");

        asignacion.Activo = !asignacion.Activo;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Estado actualizado correctamente.",
            asignacion.IdAsignacionDocente,
            asignacion.Activo
        });
    }
}