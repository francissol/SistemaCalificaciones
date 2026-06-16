using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using System.Security.Claims;

namespace SistemaCalificaciones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Maestro")]
public class PanelMaestroController : ControllerBase
{
    private readonly AppDbContext _context;

    public PanelMaestroController(AppDbContext context)
    {
        _context = context;
    }

    private int GetIdUsuario()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    [HttpGet("mis-asignaciones")]
    public async Task<IActionResult> MisAsignaciones()
    {
        var idUsuario = GetIdUsuario();

        var maestro = await _context.Maestros
            .FirstOrDefaultAsync(m => m.IdUsuario == idUsuario);

        if (maestro == null)
            return NotFound("Maestro no encontrado.");

        var asignaciones = await _context.AsignacionesDocentes
            .Include(a => a.Curso)
                .ThenInclude(c => c.Grado)
            .Include(a => a.Materia)
            .Include(a => a.AnioEscolar)
            .Where(a => a.IdMaestro == maestro.IdMaestro && a.Activo && a.AnioEscolar.Activo)
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

    [HttpGet("asignacion/{idAsignacionDocente}/estudiantes")]
    public async Task<IActionResult> EstudiantesPorAsignacion(int idAsignacionDocente)
    {
        var idUsuario = GetIdUsuario();

        var maestro = await _context.Maestros
            .FirstOrDefaultAsync(m => m.IdUsuario == idUsuario);

        if (maestro == null)
            return NotFound("Maestro no encontrado.");

        var asignacion = await _context.AsignacionesDocentes
            .FirstOrDefaultAsync(a =>
                a.IdAsignacionDocente == idAsignacionDocente &&
                a.IdMaestro == maestro.IdMaestro &&
                a.Activo);

        if (asignacion == null)
            return Unauthorized("No tienes acceso a esta asignación.");

        var estudiantes = await _context.Inscripciones
            .Include(i => i.Estudiante)
            .Where(i =>
                i.IdCurso == asignacion.IdCurso &&
                i.IdAnioEscolar == asignacion.IdAnioEscolar &&
                i.Estado == "Activo")
            .OrderBy(i => i.Estudiante.Nombres)
            .Select(i => new
            {
                i.Estudiante.IdEstudiante,
                i.Estudiante.Matricula,
                i.Estudiante.Nombres,
                i.Estudiante.Apellidos
            })
            .ToListAsync();

        return Ok(estudiantes);
    }


    [HttpGet("asignacion/{idAsignacionDocente}/detalle")]
    public async Task<IActionResult> DetalleAsignacion(int idAsignacionDocente)
    {
        var idUsuario = GetIdUsuario();

        var maestro = await _context.Maestros
            .FirstOrDefaultAsync(m => m.IdUsuario == idUsuario);

        if (maestro == null)
            return NotFound("Maestro no encontrado.");

        var asignacion = await _context.AsignacionesDocentes
            .Include(a => a.Curso)
                .ThenInclude(c => c.Grado)
                    .ThenInclude(g => g.Nivel)
            .Include(a => a.Materia)
            .Include(a => a.AnioEscolar)
            .FirstOrDefaultAsync(a =>
                a.IdAsignacionDocente == idAsignacionDocente &&
                a.IdMaestro == maestro.IdMaestro);

        if (asignacion == null)
            return Unauthorized("No tienes acceso a esta asignación.");

        return Ok(new
        {
            asignacion.IdAsignacionDocente,
            Curso = asignacion.Curso.Nombre,
            Grado = asignacion.Curso.Grado.Nombre,
            Nivel = asignacion.Curso.Grado.Nivel.Nombre,
            UsaCompetencias = asignacion.Curso.Grado.Nivel.UsaCompetencias,
            IdGrado = asignacion.Curso.IdGrado,
            IdMateria = asignacion.IdMateria,
            EsTecnica = asignacion.Materia.EsTecnica,
            Materia = asignacion.Materia.Nombre,
            AnioEscolar = asignacion.AnioEscolar.Nombre
        });
    }

    [HttpGet("asignacion/{idAsignacionDocente}/periodo/{idPeriodoPublicacion}/resumen-notas")]
    public async Task<IActionResult> ResumenNotas(int idAsignacionDocente, int idPeriodoPublicacion)
    {
        var idUsuario = GetIdUsuario();

        var maestro = await _context.Maestros
            .FirstOrDefaultAsync(m => m.IdUsuario == idUsuario);

        if (maestro == null)
            return NotFound("Maestro no encontrado.");

        var asignacion = await _context.AsignacionesDocentes
            .FirstOrDefaultAsync(a =>
                a.IdAsignacionDocente == idAsignacionDocente &&
                a.IdMaestro == maestro.IdMaestro &&
                a.Activo);

        if (asignacion == null)
            return Unauthorized("No tienes acceso a esta asignación.");

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

        var estudiantes = await _context.Inscripciones
            .Include(i => i.Estudiante)
            .Where(i =>
                i.IdCurso == asignacion.IdCurso &&
                i.IdAnioEscolar == asignacion.IdAnioEscolar &&
                i.Estado == "Activo")
            .Select(i => i.Estudiante)
            .ToListAsync();

        var resultado = new List<object>();

        foreach (var estudiante in estudiantes)
        {
            var notas = await _context.NotasActividades
                .Where(n =>
                    n.IdEstudiante == estudiante.IdEstudiante &&
                    actividades.Select(a => a.IdActividadEvaluativa).Contains(n.IdActividadEvaluativa))
                .Select(n => new
                {
                    n.IdActividadEvaluativa,
                    n.Nota
                })
                .ToListAsync();

            var notaFinal = await _context.CalificacionesPeriodo
                .Where(c =>
                    c.IdEstudiante == estudiante.IdEstudiante &&
                    c.IdAsignacionDocente == idAsignacionDocente &&
                    c.IdPeriodoPublicacion == idPeriodoPublicacion)
                .Select(c => new
                {
                    c.NotaFinal,
                    c.Publicada
                })
                .FirstOrDefaultAsync();

            resultado.Add(new
            {
                estudiante.IdEstudiante,
                Estudiante = estudiante.Nombres + " " + estudiante.Apellidos,
                Notas = notas,
                NotaFinal = notaFinal?.NotaFinal,
                Publicada = notaFinal?.Publicada ?? false
            });
        }

        return Ok(new
        {
            Actividades = actividades,
            Estudiantes = resultado
        });
    }
}