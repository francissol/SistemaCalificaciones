using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.Models;

namespace SistemaCalificaciones.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize(Roles = "Administrador,Maestro")]
public class CalificacionesCompetenciasPeriodoController : ControllerBase
{
    private readonly AppDbContext _context;

    public CalificacionesCompetenciasPeriodoController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("calcular")]
    public async Task<IActionResult> Calcular(int idAsignacionDocente, int idPeriodoPublicacion)
    {
        var asignacion = await _context.AsignacionesDocentes
            .Include(a => a.Curso)
                .ThenInclude(c => c.Grado)
                    .ThenInclude(g => g.Nivel)
            .Include(a => a.Materia)
            .FirstOrDefaultAsync(a => a.IdAsignacionDocente == idAsignacionDocente);

        if (asignacion == null)
            return BadRequest("Asignación docente no encontrada.");

        if (asignacion.Materia.EsTecnica)
            return BadRequest("Las materias técnicas usan Resultados de Aprendizaje.");

        var estudiantes = await _context.Inscripciones
            .Where(i =>
                i.IdCurso == asignacion.IdCurso &&
                i.IdAnioEscolar == asignacion.IdAnioEscolar &&
                i.Estado == "Activo")
            .Select(i => i.Estudiante)
            .ToListAsync();

        var competencias = await _context.CompetenciasGradoMateria
            .Where(cgm =>
                cgm.IdGrado == asignacion.Curso.IdGrado &&
                cgm.IdMateria == asignacion.IdMateria)
            .Select(cgm => cgm.Competencia)
            .ToListAsync();

        if (!competencias.Any())
            return BadRequest("No hay competencias asignadas a esta materia y grado.");

        foreach (var estudiante in estudiantes)
        {
            foreach (var competencia in competencias)
            {
                var actividades = await _context.ActividadesCompetencias
                    .Where(a =>
                        a.IdAsignacionDocente == idAsignacionDocente &&
                        a.IdPeriodoPublicacion == idPeriodoPublicacion &&
                        a.IdCompetencia == competencia.IdCompetencia &&
                        a.IdResultadoAprendizaje == null &&
                        a.Activa)
                    .ToListAsync();

                var idsActividades = actividades
                    .Select(a => a.IdActividadCompetencia)
                    .ToList();

                var notas = await _context.NotasCompetencias
                    .Where(n =>
                        idsActividades.Contains(n.IdActividadCompetencia) &&
                        n.IdEstudiante == estudiante.IdEstudiante)
                    .ToListAsync();

                if (!notas.Any())
                    continue;

                var promedio = Math.Round(notas.Average(n => n.Nota), 2);

                var calificacion = await _context.CalificacionesCompetenciasPeriodo
                    .FirstOrDefaultAsync(c =>
                        c.IdEstudiante == estudiante.IdEstudiante &&
                        c.IdAsignacionDocente == idAsignacionDocente &&
                        c.IdPeriodoPublicacion == idPeriodoPublicacion &&
                        c.IdCompetencia == competencia.IdCompetencia);

                if (calificacion == null)
                {
                    calificacion = new CalificacionCompetenciaPeriodo
                    {
                        IdEstudiante = estudiante.IdEstudiante,
                        IdAsignacionDocente = idAsignacionDocente,
                        IdPeriodoPublicacion = idPeriodoPublicacion,
                        IdCompetencia = competencia.IdCompetencia,
                        Promedio = promedio,
                        Publicada = false,
                        FechaRegistro = DateTime.Now
                    };

                    _context.CalificacionesCompetenciasPeriodo.Add(calificacion);
                }
                else
                {
                    calificacion.Promedio = promedio;
                    calificacion.FechaRegistro = DateTime.Now;
                }
            }
        }

        await _context.SaveChangesAsync();

        return Ok("Calificaciones por competencias calculadas correctamente.");
    }

    [HttpPut("publicar")]
    public async Task<IActionResult> Publicar(int idAsignacionDocente, int idPeriodoPublicacion)
    {
        var calificaciones = await _context.CalificacionesCompetenciasPeriodo
            .Where(c =>
                c.IdAsignacionDocente == idAsignacionDocente &&
                c.IdPeriodoPublicacion == idPeriodoPublicacion)
            .ToListAsync();

        if (!calificaciones.Any())
            return BadRequest("Primero debes calcular las calificaciones por competencias.");

        foreach (var calificacion in calificaciones)
        {
            calificacion.Publicada = true;
            calificacion.FechaPublicacion = DateTime.Now;
        }

        await _context.SaveChangesAsync();

        return Ok("Calificaciones por competencias publicadas correctamente.");
    }

    [HttpGet("estudiante/{idEstudiante}")]
  //  [Authorize(Roles = "Administrador,Maestro,Estudiante,Padre")]
    public async Task<IActionResult> GetPorEstudiante(int idEstudiante)
    {
        var calificaciones = await _context.CalificacionesCompetenciasPeriodo
            .Include(c => c.AsignacionDocente)
                .ThenInclude(a => a.Materia)
            .Include(c => c.PeriodoPublicacion)
            .Include(c => c.Competencia)
            .Where(c => c.IdEstudiante == idEstudiante && c.Publicada)
            .OrderBy(c => c.AsignacionDocente.Materia.Nombre)
            .ThenBy(c => c.Competencia.Codigo)
            .Select(c => new
            {
                c.IdCalificacionCompetenciaPeriodo,
                c.IdAsignacionDocente,
                c.IdPeriodoPublicacion,
                c.IdCompetencia,
                Materia = c.AsignacionDocente.Materia.Nombre,
                Periodo = c.PeriodoPublicacion.Nombre,
                Competencia = c.Competencia.Codigo,
                NombreCompetencia = c.Competencia.Nombre,
                c.Promedio,
                c.FechaPublicacion
            })
            .ToListAsync();

        return Ok(calificaciones);
    }
}