using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.DTOs.Observaciones;
using SistemaCalificaciones.Models;

namespace SistemaCalificaciones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ObservacionesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ObservacionesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("estudiante/{idEstudiante}")]
    [Authorize(Roles = "Administrador,Maestro,Estudiante,Padre")]
    public async Task<IActionResult> GetPorEstudiante(int idEstudiante)
    {
        var observaciones = await _context.Observaciones
            .Include(o => o.AsignacionDocente)
                .ThenInclude(a => a.Materia)
            .Include(o => o.PeriodoPublicacion)
            .Where(o => o.IdEstudiante == idEstudiante)
            .OrderByDescending(o => o.FechaRegistro)
            .Select(o => new
            {
                o.IdObservacion,
                o.IdEstudiante,
                Materia = o.AsignacionDocente.Materia.Nombre,
                Periodo = o.PeriodoPublicacion.Nombre,
                o.Tipo,
                o.Comentario,
                o.FechaRegistro
            })
            .ToListAsync();

        return Ok(observaciones);
    }

    [HttpGet("padres/estudiante/{idEstudiante}")]
    [Authorize(Roles = "Administrador,Maestro,Padre")]
    public async Task<IActionResult> GetObservacionesPadres(int idEstudiante)
    {
        var observaciones = await _context.Observaciones
            .Include(o => o.AsignacionDocente)
                .ThenInclude(a => a.Materia)
            .Include(o => o.PeriodoPublicacion)
            .Where(o => o.IdEstudiante == idEstudiante && o.Tipo == "PADRE")
            .OrderByDescending(o => o.FechaRegistro)
            .Select(o => new
            {
                o.IdObservacion,
                Materia = o.AsignacionDocente.Materia.Nombre,
                Periodo = o.PeriodoPublicacion.Nombre,
                o.Comentario,
                o.FechaRegistro
            })
            .ToListAsync();

        return Ok(observaciones);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Maestro")]
    public async Task<IActionResult> Crear(CrearObservacionDto dto)
    {
        var tiposPermitidos = new[] { "ESTUDIANTE", "PADRE" };

        dto.Tipo = dto.Tipo.Trim().ToUpper();

        if (!tiposPermitidos.Contains(dto.Tipo))
            return BadRequest("El tipo debe ser ESTUDIANTE o PADRE.");

        var estudianteExiste = await _context.Estudiantes
            .AnyAsync(e => e.IdEstudiante == dto.IdEstudiante && e.Activo);

        if (!estudianteExiste)
            return BadRequest("El estudiante no existe o está inactivo.");

        var asignacionExiste = await _context.AsignacionesDocentes
            .AnyAsync(a => a.IdAsignacionDocente == dto.IdAsignacionDocente && a.Activo);

        if (!asignacionExiste)
            return BadRequest("La asignación docente no existe o está inactiva.");

        var periodo = await _context.PeriodosPublicacion
            .FirstOrDefaultAsync(p => p.IdPeriodoPublicacion == dto.IdPeriodoPublicacion);

        if (periodo == null || !periodo.Activo)
            return BadRequest("El período no existe o está inactivo.");

        if (DateTime.Now.Date > periodo.FechaCierre.Date)
            return BadRequest("El período ya cerró. No puedes registrar observaciones.");

        var observacion = new Observacion
        {
            IdEstudiante = dto.IdEstudiante,
            IdAsignacionDocente = dto.IdAsignacionDocente,
            IdPeriodoPublicacion = dto.IdPeriodoPublicacion,
            Tipo = dto.Tipo,
            Comentario = dto.Comentario,
            FechaRegistro = DateTime.Now
        };

        _context.Observaciones.Add(observacion);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Observación registrada correctamente.",
            observacion.IdObservacion,
            observacion.Tipo,
            observacion.Comentario
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador,Maestro")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var observacion = await _context.Observaciones.FindAsync(id);

        if (observacion == null)
            return NotFound("Observación no encontrada.");

        _context.Observaciones.Remove(observacion);
        await _context.SaveChangesAsync();

        return Ok("Observación eliminada correctamente.");
    }
}