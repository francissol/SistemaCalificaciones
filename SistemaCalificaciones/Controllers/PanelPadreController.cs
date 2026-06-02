using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using System.Security.Claims;

namespace SistemaCalificaciones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Padre")]
public class PanelPadreController : ControllerBase
{
    private readonly AppDbContext _context;

    public PanelPadreController(AppDbContext context)
    {
        _context = context;
    }

    private int GetIdUsuario()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    [HttpGet("mis-hijos")]
    public async Task<IActionResult> MisHijos()
    {
        var idUsuario = GetIdUsuario();

        var padre = await _context.Padres
            .FirstOrDefaultAsync(p => p.IdUsuario == idUsuario);

        if (padre == null)
            return NotFound("Padre no encontrado.");

        var hijos = await _context.PadreEstudiantes
            .Include(pe => pe.Estudiante)
                .ThenInclude(e => e.Inscripciones)
                    .ThenInclude(i => i.Curso)
            .Where(pe => pe.IdPadre == padre.IdPadre)
            .Select(pe => new
            {
                pe.Estudiante.IdEstudiante,
                pe.Estudiante.Matricula,
                pe.Estudiante.Nombres,
                pe.Estudiante.Apellidos,
                pe.Parentesco,

                CursoActual = pe.Estudiante.Inscripciones
                    .OrderByDescending(i => i.IdInscripcion)
                    .Select(i => i.Curso.Nombre)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(hijos);
    }

    [HttpGet("hijo/{idEstudiante}/calificaciones")]
    public async Task<IActionResult> CalificacionesHijo(int idEstudiante)
    {
        var calificaciones = await _context.CalificacionesPeriodo
            .Include(c => c.AsignacionDocente)
                .ThenInclude(a => a.Materia)
            .Include(c => c.PeriodoPublicacion)
            .Where(c =>
                c.IdEstudiante == idEstudiante &&
                c.Publicada)
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
    private int ObtenerIdUsuario()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
    }

    [HttpGet("hijo/{idEstudiante}/desglose-calificacion")]
    public async Task<IActionResult> DesgloseCalificacionHijo(
    int idEstudiante,
    int idAsignacionDocente,
    int idPeriodoPublicacion)
    {
        var idUsuario = ObtenerIdUsuario();

        var padre = await _context.Padres
            .FirstOrDefaultAsync(p => p.IdUsuario == idUsuario);

        if (padre == null)
            return NotFound("Padre no encontrado.");

        var esSuHijo = await _context.PadreEstudiantes
            .AnyAsync(pe =>
                pe.IdPadre == padre.IdPadre &&
                pe.IdEstudiante == idEstudiante);

        if (!esSuHijo)
            return Unauthorized("No tienes permiso para ver este estudiante.");

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
                    n.IdEstudiante == idEstudiante);

            decimal notaActividad = nota?.Nota ?? 0;
            decimal valorCalculado = notaActividad * (actividad.Porcentaje / 100);

            total += valorCalculado;

            desglose.Add(new
            {
                Actividad = actividad.Nombre,
                actividad.Porcentaje,
                Nota = notaActividad,
                ValorCalculado = Math.Round(valorCalculado, 2)
            });
        }

        return Ok(new
        {
            NotaFinal = Math.Round(total, 2),
            Desglose = desglose
        });
    }

    [HttpGet("hijo/{idEstudiante}/observaciones")]
    public async Task<IActionResult> ObservacionesHijo(int idEstudiante)
    {
        var observaciones = await _context.Observaciones
            .Include(o => o.AsignacionDocente)
                .ThenInclude(a => a.Materia)
            .Include(o => o.PeriodoPublicacion)
            .Where(o =>
                o.IdEstudiante == idEstudiante &&
                o.Tipo == "PADRE")
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