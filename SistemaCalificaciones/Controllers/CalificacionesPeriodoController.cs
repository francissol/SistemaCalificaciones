using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.Models;

namespace SistemaCalificaciones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,CoordinadorPrimaria,CoordinadorSecundaria,CoordinadorPolitecnico,Maestro")]
public class CalificacionesPeriodoController : ControllerBase
{
    private readonly AppDbContext _context;

    public CalificacionesPeriodoController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("calcular")]
    public async Task<IActionResult> Calcular(int idAsignacionDocente, int idPeriodoPublicacion)
    {
        var actividades = await _context.ActividadesEvaluativas
            .Where(a =>
                a.IdAsignacionDocente == idAsignacionDocente &&
                a.IdPeriodoPublicacion == idPeriodoPublicacion &&
                a.Activa)
            .ToListAsync();

        if (!actividades.Any())
            return BadRequest("No hay actividades registradas.");

        var totalPorcentaje = actividades.Sum(a => a.Porcentaje);

        if (totalPorcentaje != 100)
            return BadRequest($"Las actividades deben sumar 100%. Actualmente suman {totalPorcentaje}%.");

        var asignacion = await _context.AsignacionesDocentes
            .Include(a => a.Curso)
            .FirstOrDefaultAsync(a => a.IdAsignacionDocente == idAsignacionDocente);

        if (asignacion == null)
            return BadRequest("Asignación docente no encontrada.");

        var estudiantes = await _context.Inscripciones
            .Where(i =>
                i.IdCurso == asignacion.IdCurso &&
                i.IdAnioEscolar == asignacion.IdAnioEscolar &&
                i.Estado == "Activo")
            .Select(i => i.Estudiante)
            .ToListAsync();

        foreach (var estudiante in estudiantes)
        {
            decimal notaFinal = 0;

            foreach (var actividad in actividades)
            {
                var notaActividad = await _context.NotasActividades
                    .FirstOrDefaultAsync(n =>
                        n.IdActividadEvaluativa == actividad.IdActividadEvaluativa &&
                        n.IdEstudiante == estudiante.IdEstudiante);

                if (notaActividad != null)
                {
                    notaFinal += notaActividad.Nota * (actividad.Porcentaje / 100);
                }
            }

            var calificacion = await _context.CalificacionesPeriodo
                .FirstOrDefaultAsync(c =>
                    c.IdEstudiante == estudiante.IdEstudiante &&
                    c.IdAsignacionDocente == idAsignacionDocente &&
                    c.IdPeriodoPublicacion == idPeriodoPublicacion);

            if (calificacion == null)
            {
                calificacion = new CalificacionPeriodo
                {
                    IdEstudiante = estudiante.IdEstudiante,
                    IdAsignacionDocente = idAsignacionDocente,
                    IdPeriodoPublicacion = idPeriodoPublicacion,
                    NotaFinal = Math.Round(notaFinal, 2),
                    Publicada = false,
                    FechaRegistro = DateTime.Now
                };

                _context.CalificacionesPeriodo.Add(calificacion);
            }
            else
            {
                calificacion.NotaFinal = Math.Round(notaFinal, 2);
            }
        }

        await _context.SaveChangesAsync();

        return Ok("Calificaciones calculadas correctamente.");
    }

    [HttpPut("publicar")]
    public async Task<IActionResult> Publicar(int idAsignacionDocente, int idPeriodoPublicacion)
    {
        var calificaciones = await _context.CalificacionesPeriodo
            .Where(c =>
                c.IdAsignacionDocente == idAsignacionDocente &&
                c.IdPeriodoPublicacion == idPeriodoPublicacion)
            .ToListAsync();

        if (!calificaciones.Any())
            return BadRequest("Primero debes calcular las calificaciones.");

        foreach (var calificacion in calificaciones)
        {
            calificacion.Publicada = true;
            calificacion.FechaPublicacion = DateTime.Now;
        }

        await _context.SaveChangesAsync();

        return Ok("Calificaciones publicadas correctamente.");
    }

    [HttpGet("estudiante/{idEstudiante}")]
    [Authorize(Roles = "Administrador,Maestro,Estudiante,Padre")]
    public async Task<IActionResult> GetPorEstudiante(int idEstudiante)
    {
        var calificaciones = await _context.CalificacionesPeriodo
            .Include(c => c.AsignacionDocente)
                .ThenInclude(a => a.Materia)
            .Include(c => c.PeriodoPublicacion)
            .Where(c => c.IdEstudiante == idEstudiante && c.Publicada)
            .OrderBy(c => c.AsignacionDocente.Materia.Nombre)
            .ThenBy(c => c.PeriodoPublicacion.FechaInicio)
            .Select(c => new
            {
                c.IdCalificacionPeriodo,
                c.IdAsignacionDocente,
                c.IdPeriodoPublicacion,
                Materia = c.AsignacionDocente.Materia.Nombre,
                Periodo = c.PeriodoPublicacion.Nombre,
                c.NotaFinal,
                c.FechaPublicacion
            })
            .ToListAsync();

        return Ok(calificaciones);
    }
}