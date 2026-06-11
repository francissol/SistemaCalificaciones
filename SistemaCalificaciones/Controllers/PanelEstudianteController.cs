using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using System.Security.Claims;

namespace SistemaCalificaciones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Estudiante")]
public class PanelEstudianteController : ControllerBase
{
    private readonly AppDbContext _context;

    public PanelEstudianteController(AppDbContext context)
    {
        _context = context;
    }

    private int GetIdUsuario()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    [HttpGet("mi-perfil")]
    public async Task<IActionResult> MiPerfil()
    {
        var idUsuario = GetIdUsuario();

        var estudiante = await _context.Estudiantes
            .Include(e => e.Inscripciones)
                .ThenInclude(i => i.Curso)
                    .ThenInclude(c => c.Grado)
                        .ThenInclude(g => g.Nivel)
            .FirstOrDefaultAsync(e => e.IdUsuario == idUsuario);

        if (estudiante == null)
            return NotFound("Estudiante no encontrado.");

        var inscripcionActual = estudiante.Inscripciones
            .OrderByDescending(i => i.IdInscripcion)
            .FirstOrDefault();

        return Ok(new
        {
            estudiante.IdEstudiante,
            estudiante.Matricula,
            estudiante.Nombres,
            estudiante.Apellidos,
            estudiante.Correo,
            estudiante.Telefono,
            Curso = inscripcionActual?.Curso.Nombre,
            Grado = inscripcionActual?.Curso.Grado.Nombre,
            Nivel = inscripcionActual?.Curso.Grado.Nivel.Nombre,
            UsaCompetencias = inscripcionActual?.Curso.Grado.Nivel.UsaCompetencias ?? false
        });
    }

    [HttpGet("desglose-calificacion")]
    public async Task<IActionResult> DesgloseCalificacion(int idAsignacionDocente, int idPeriodoPublicacion, int? idEstudianteOpcional = null)
    {
        var idUsuario = int.Parse(
    User.FindFirst(ClaimTypes.NameIdentifier)?.Value!
);

        var estudiante = await _context.Estudiantes
            .FirstOrDefaultAsync(e => e.IdUsuario == idUsuario);

        if (estudiante == null)
            return NotFound("Estudiante no encontrado.");

        var actividades = await _context.ActividadesEvaluativas
            .Where(a =>
                a.IdAsignacionDocente == idAsignacionDocente &&
                a.IdPeriodoPublicacion == idPeriodoPublicacion &&
                a.Activa)
            .Select(a => new
            {
                a.IdActividadEvaluativa,
                a.Nombre,
                a.Porcentaje
            })
            .ToListAsync();

        var desglose = new List<object>();

        decimal total = 0;

        foreach (var actividad in actividades)
        {
            var nota = await _context.NotasActividades
                .FirstOrDefaultAsync(n =>
                    n.IdActividadEvaluativa == actividad.IdActividadEvaluativa &&
                    n.IdEstudiante == estudiante.IdEstudiante);

            decimal notaActividad = nota?.Nota ?? 0;
            decimal valor = notaActividad * (actividad.Porcentaje / 100);

            total += valor;

            desglose.Add(new
            {
                Actividad = actividad.Nombre,
                actividad.Porcentaje,
                Nota = notaActividad,
                ValorCalculado = Math.Round(valor, 2)
            });
        }

        return Ok(new
        {
            NotaFinal = Math.Round(total, 2),
            Desglose = desglose
        });
    }

    [HttpGet("mis-materias")]
    public async Task<IActionResult> MisMaterias()
    {
        var idUsuario = GetIdUsuario();

        var estudiante = await _context.Estudiantes
            .Include(e => e.Inscripciones)
            .FirstOrDefaultAsync(e => e.IdUsuario == idUsuario);

        if (estudiante == null)
            return NotFound("Estudiante no encontrado.");

        var inscripcionActual = estudiante.Inscripciones
            .OrderByDescending(i => i.IdInscripcion)
            .FirstOrDefault();

        if (inscripcionActual == null)
            return NotFound("El estudiante no tiene inscripción activa.");

        var materias = await _context.AsignacionesDocentes
            .Include(a => a.Materia)
            .Include(a => a.Maestro)
            .Where(a =>
                a.IdCurso == inscripcionActual.IdCurso &&
                a.IdAnioEscolar == inscripcionActual.IdAnioEscolar &&
                a.Activo)
            .Select(a => new
            {
                a.IdAsignacionDocente,
                Materia = a.Materia.Nombre,
                Maestro = a.Maestro.Nombres + " " + a.Maestro.Apellidos
            })
            .ToListAsync();

        return Ok(materias);
    }

    [HttpGet("mis-calificaciones")]
    public async Task<IActionResult> MisCalificaciones()
    {
        var idUsuario = GetIdUsuario();

        var estudiante = await _context.Estudiantes
            .FirstOrDefaultAsync(e => e.IdUsuario == idUsuario);

        if (estudiante == null)
            return NotFound("Estudiante no encontrado.");

        var calificaciones = await _context.CalificacionesPeriodo
            .Include(c => c.AsignacionDocente)
                .ThenInclude(a => a.Materia)
            .Include(c => c.PeriodoPublicacion)
            .Where(c => c.IdEstudiante == estudiante.IdEstudiante && c.Publicada)
            .OrderBy(c => c.AsignacionDocente.Materia.Nombre)
            .ThenBy(c => c.PeriodoPublicacion.FechaInicio)
            .Select(c => new
            {
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

    [HttpGet("progreso-anual-primaria")]
    public async Task<IActionResult> ProgresoAnualPrimaria()
    {
        var idUsuario = int.Parse(
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!
        );

        var estudiante = await _context.Estudiantes
            .FirstOrDefaultAsync(e => e.IdUsuario == idUsuario);

        if (estudiante == null)
            return NotFound("Estudiante no encontrado.");

        var calificaciones = await _context.CalificacionesCompetenciasPeriodo
            .Include(c => c.AsignacionDocente)
                .ThenInclude(a => a.Materia)
            .Include(c => c.PeriodoPublicacion)
            .Include(c => c.Competencia)
            .Where(c => c.IdEstudiante == estudiante.IdEstudiante && c.Publicada)
            .ToListAsync();

        var resultado = calificaciones
            .GroupBy(c => new
            {
                c.IdAsignacionDocente,
                Materia = c.AsignacionDocente.Materia.Nombre
            })
            .Select(materiaGrupo =>
            {
                var competencias = materiaGrupo
                    .GroupBy(c => new
                    {
                        c.IdCompetencia,
                        Codigo = c.Competencia.Codigo,
                        Nombre = c.Competencia.Nombre
                    })
                    .Select(compGrupo =>
                    {
                        var periodos = compGrupo
                            .OrderBy(x => x.PeriodoPublicacion.FechaInicio)
                            .Select(x => new
                            {
                                Periodo = x.PeriodoPublicacion.Nombre,
                                Promedio = x.Promedio
                            })
                            .ToList();

                        var promedioCompetencia = periodos.Any()
                            ? Math.Round(periodos.Average(p => p.Promedio), 2)
                            : 0;

                        return new
                        {
                            compGrupo.Key.IdCompetencia,
                            compGrupo.Key.Codigo,
                            compGrupo.Key.Nombre,
                            Periodos = periodos,
                            PromedioCompetencia = promedioCompetencia
                        };
                    })
                    .OrderBy(c => c.Codigo)
                    .ToList();

                var finalArea = competencias.Any()
                    ? Math.Round(competencias.Average(c => c.PromedioCompetencia), 2)
                    : 0;

                return new
                {
                    materiaGrupo.Key.IdAsignacionDocente,
                    materiaGrupo.Key.Materia,
                    Competencias = competencias,
                    FinalArea = finalArea
                };
            })
            .ToList();

        return Ok(resultado);
    }

    [HttpGet("mis-calificaciones-primaria")]
    public async Task<IActionResult> MisCalificacionesPrimaria()
    {
        var idUsuario = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);

        var estudiante = await _context.Estudiantes
            .FirstOrDefaultAsync(e => e.IdUsuario == idUsuario);

        if (estudiante == null)
            return NotFound("Estudiante no encontrado.");

        var calificaciones = await _context.CalificacionesCompetenciasPeriodo
            .Include(c => c.AsignacionDocente)
                .ThenInclude(a => a.Materia)
            .Include(c => c.PeriodoPublicacion)
            .Include(c => c.Competencia)
            .Where(c => c.IdEstudiante == estudiante.IdEstudiante && c.Publicada)
            .ToListAsync();

        var resultado = calificaciones
            .GroupBy(c => new
            {
                c.IdAsignacionDocente,
                c.IdPeriodoPublicacion,
                Materia = c.AsignacionDocente.Materia.Nombre,
                Periodo = c.PeriodoPublicacion.Nombre,
                FechaPublicacion = c.FechaPublicacion
            })
            .Select(g => new
            {
                g.Key.IdAsignacionDocente,
                g.Key.IdPeriodoPublicacion,
                g.Key.Materia,
                g.Key.Periodo,
                NotaFinal = Math.Round(g.Average(x => x.Promedio), 2),
                g.Key.FechaPublicacion,
                Competencias = g
                    .OrderBy(x => x.Competencia.Codigo)
                    .Select(x => new
                    {
                        x.IdCompetencia,
                        Codigo = x.Competencia.Codigo,
                        Nombre = x.Competencia.Nombre,
                        Promedio = x.Promedio
                    })
                    .ToList()
            })
            .ToList();

        return Ok(resultado);
    }

    [HttpGet("desglose-primaria")]
    public async Task<IActionResult> DesglosePrimaria(
    int idAsignacionDocente,
    int idPeriodoPublicacion)
    {
        var idUsuario = int.Parse(
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!
        );

        var estudiante = await _context.Estudiantes
            .FirstOrDefaultAsync(e => e.IdUsuario == idUsuario);

        if (estudiante == null)
            return NotFound("Estudiante no encontrado.");

        var asignacion = await _context.AsignacionesDocentes
            .Include(a => a.Materia)
            .FirstOrDefaultAsync(a => a.IdAsignacionDocente == idAsignacionDocente);

        if (asignacion == null)
            return NotFound("Asignación no encontrada.");

        var periodo = await _context.PeriodosPublicacion
            .FirstOrDefaultAsync(p => p.IdPeriodoPublicacion == idPeriodoPublicacion);

        if (periodo == null)
            return NotFound("Período no encontrado.");

        var competencias = await _context.CalificacionesCompetenciasPeriodo
            .Include(c => c.Competencia)
            .Where(c =>
                c.IdEstudiante == estudiante.IdEstudiante &&
                c.IdAsignacionDocente == idAsignacionDocente &&
                c.IdPeriodoPublicacion == idPeriodoPublicacion &&
                c.Publicada)
            .OrderBy(c => c.Competencia.Codigo)
            .ToListAsync();

        var resultado = new List<object>();

        foreach (var comp in competencias)
        {
            var actividades = await _context.ActividadesCompetencias
                .Where(a =>
                    a.IdAsignacionDocente == idAsignacionDocente &&
                    a.IdPeriodoPublicacion == idPeriodoPublicacion &&
                    a.IdCompetencia == comp.IdCompetencia &&
                    a.Activa)
                .Select(a => new
                {
                    a.IdActividadCompetencia,
                    a.Nombre
                })
                .ToListAsync();

            var actividadesConNotas = new List<object>();

            foreach (var actividad in actividades)
            {
                var nota = await _context.NotasCompetencias
                    .FirstOrDefaultAsync(n =>
                        n.IdActividadCompetencia == actividad.IdActividadCompetencia &&
                        n.IdEstudiante == estudiante.IdEstudiante);

                actividadesConNotas.Add(new
                {
                    Actividad = actividad.Nombre,
                    Nota = nota?.Nota ?? 0
                });
            }

            resultado.Add(new
            {
                comp.IdCompetencia,
                Codigo = comp.Competencia.Codigo,
                Nombre = comp.Competencia.Nombre,
                Promedio = comp.Promedio,
                Actividades = actividadesConNotas
            });
        }

        var finalPeriodo = competencias.Any()
            ? Math.Round(competencias.Average(c => c.Promedio), 2)
            : 0;

        return Ok(new
        {
            Materia = asignacion.Materia.Nombre,
            Periodo = periodo.Nombre,
            FinalPeriodo = finalPeriodo,
            Competencias = resultado
        });
    }

    [HttpGet("mis-observaciones")]
    public async Task<IActionResult> MisObservaciones()
    {
        var idUsuario = GetIdUsuario();

        var estudiante = await _context.Estudiantes
            .FirstOrDefaultAsync(e => e.IdUsuario == idUsuario);

        if (estudiante == null)
            return NotFound("Estudiante no encontrado.");

        var observaciones = await _context.Observaciones
            .Include(o => o.AsignacionDocente)
                .ThenInclude(a => a.Materia)
            .Include(o => o.PeriodoPublicacion)
            .Where(o =>
                o.IdEstudiante == estudiante.IdEstudiante &&
                o.Tipo == "ESTUDIANTE")
            .OrderByDescending(o => o.FechaRegistro)
            .Select(o => new
            {
                Materia = o.AsignacionDocente.Materia.Nombre,
                Periodo = o.PeriodoPublicacion.Nombre,
                o.Comentario,
                o.FechaRegistro
            })
            .ToListAsync();

        return Ok(observaciones);
    }
}