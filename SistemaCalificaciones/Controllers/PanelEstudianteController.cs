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
            Grado = inscripcionActual?.Curso.Grado.Nombre
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