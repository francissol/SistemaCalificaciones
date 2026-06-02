using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;

namespace SistemaCalificaciones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class ReportesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReportesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("boletin-estudiante/{idEstudiante}")]
    public async Task<IActionResult> BoletinEstudiante(int idEstudiante)
    {
        var estudiante = await _context.Estudiantes
            .Include(e => e.Inscripciones)
                .ThenInclude(i => i.Curso)
            .FirstOrDefaultAsync(e => e.IdEstudiante == idEstudiante);

        if (estudiante == null)
            return NotFound("Estudiante no encontrado.");

        var calificaciones = await _context.CalificacionesPeriodo
            .Include(c => c.AsignacionDocente)
                .ThenInclude(a => a.Materia)
            .Include(c => c.PeriodoPublicacion)
            .Where(c => c.IdEstudiante == idEstudiante && c.Publicada)
            .OrderBy(c => c.AsignacionDocente.Materia.Nombre)
            .ThenBy(c => c.PeriodoPublicacion.FechaInicio)
            .Select(c => new
            {
                Materia = c.AsignacionDocente.Materia.Nombre,
                Periodo = c.PeriodoPublicacion.Nombre,
                Nota = c.NotaFinal
            })
            .ToListAsync();

        var promedio = calificaciones.Any()
            ? Math.Round(calificaciones.Average(c => c.Nota), 2)
            : 0;

        var cursoActual = estudiante.Inscripciones
            .OrderByDescending(i => i.IdInscripcion)
            .Select(i => i.Curso.Nombre)
            .FirstOrDefault();

        return Ok(new
        {
            estudiante.IdEstudiante,
            estudiante.Matricula,
            Estudiante = estudiante.Nombres + " " + estudiante.Apellidos,
            Curso = cursoActual,
            PromedioGeneral = promedio,
            Calificaciones = calificaciones
        });
    }

    [HttpGet("calificaciones-curso")]
    public async Task<IActionResult> CalificacionesPorCurso(int idCurso, int idPeriodoPublicacion)
    {
        var curso = await _context.Cursos.FindAsync(idCurso);

        if (curso == null)
            return NotFound("Curso no encontrado.");

        var estudiantes = await _context.Inscripciones
            .Include(i => i.Estudiante)
            .Where(i => i.IdCurso == idCurso && i.Estado == "Activo")
            .Select(i => i.Estudiante)
            .ToListAsync();

        var resultado = new List<object>();

        foreach (var estudiante in estudiantes)
        {
            var notas = await _context.CalificacionesPeriodo
                .Include(c => c.AsignacionDocente)
                    .ThenInclude(a => a.Materia)
                .Where(c =>
                    c.IdEstudiante == estudiante.IdEstudiante &&
                    c.IdPeriodoPublicacion == idPeriodoPublicacion &&
                    c.Publicada)
                .Select(c => new
                {
                    Materia = c.AsignacionDocente.Materia.Nombre,
                    Nota = c.NotaFinal
                })
                .ToListAsync();

            resultado.Add(new
            {
                estudiante.IdEstudiante,
                estudiante.Matricula,
                Estudiante = estudiante.Nombres + " " + estudiante.Apellidos,
                Calificaciones = notas,
                Promedio = notas.Any() ? Math.Round(notas.Average(n => n.Nota), 2) : 0
            });
        }

        return Ok(new
        {
            Curso = curso.Nombre,
            IdPeriodoPublicacion = idPeriodoPublicacion,
            Estudiantes = resultado
        });
    }

    [HttpGet("maestros-pendientes")]
    public async Task<IActionResult> MaestrosPendientes(int idPeriodoPublicacion)
    {
        var periodo = await _context.PeriodosPublicacion.FindAsync(idPeriodoPublicacion);

        if (periodo == null)
            return NotFound("Período no encontrado.");

        var asignaciones = await _context.AsignacionesDocentes
            .Include(a => a.Maestro)
            .Include(a => a.Curso)
            .Include(a => a.Materia)
            .Where(a => a.IdAnioEscolar == periodo.IdAnioEscolar && a.Activo)
            .ToListAsync();

        var resultado = new List<object>();

        foreach (var asignacion in asignaciones)
        {
            var totalEstudiantes = await _context.Inscripciones
                .CountAsync(i =>
                    i.IdCurso == asignacion.IdCurso &&
                    i.IdAnioEscolar == asignacion.IdAnioEscolar &&
                    i.Estado == "Activo");

            var totalPublicadas = await _context.CalificacionesPeriodo
                .CountAsync(c =>
                    c.IdAsignacionDocente == asignacion.IdAsignacionDocente &&
                    c.IdPeriodoPublicacion == idPeriodoPublicacion &&
                    c.Publicada);

            bool completo = totalEstudiantes > 0 && totalPublicadas >= totalEstudiantes;

            resultado.Add(new
            {
                Maestro = asignacion.Maestro.Nombres + " " + asignacion.Maestro.Apellidos,
                Curso = asignacion.Curso.Nombre,
                Materia = asignacion.Materia.Nombre,
                TotalEstudiantes = totalEstudiantes,
                TotalPublicadas = totalPublicadas,
                Estado = completo ? "Completado" : "Pendiente"
            });
        }

        return Ok(new
        {
            Periodo = periodo.Nombre,
            TotalAsignaciones = resultado.Count,
            Completadas = resultado.Count(r => r.ToString()!.Contains("Completado")),
            Pendientes = resultado.Count(r => r.ToString()!.Contains("Pendiente")),
            Detalle = resultado
        });
    }

    [HttpGet("observaciones-estudiante/{idEstudiante}")]
    public async Task<IActionResult> ObservacionesEstudiante(int idEstudiante)
    {
        var estudiante = await _context.Estudiantes.FindAsync(idEstudiante);

        if (estudiante == null)
            return NotFound("Estudiante no encontrado.");

        var observaciones = await _context.Observaciones
            .Include(o => o.AsignacionDocente)
                .ThenInclude(a => a.Materia)
            .Include(o => o.PeriodoPublicacion)
            .Where(o => o.IdEstudiante == idEstudiante)
            .OrderByDescending(o => o.FechaRegistro)
            .Select(o => new
            {
                Materia = o.AsignacionDocente.Materia.Nombre,
                Periodo = o.PeriodoPublicacion.Nombre,
                o.Tipo,
                o.Comentario,
                o.FechaRegistro
            })
            .ToListAsync();

        return Ok(new
        {
            Estudiante = estudiante.Nombres + " " + estudiante.Apellidos,
            Observaciones = observaciones
        });
    }
}