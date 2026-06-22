using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.Helpers;
using SistemaCalificaciones.Models;
using SistemaCalificaciones.Models;

namespace SistemaCalificaciones.Controllers;



[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador, Maestro,CoordinadorPrimaria,CoordinadorSecundaria,CoordinadorPolitecnico")]
public class ReportesController : ControllerBase
{
    private readonly AppDbContext _context;

    public class CompetenciaAnualDto
    {
        public decimal? P1 { get; set; }
        public decimal? RP1 { get; set; }

        public decimal? P2 { get; set; }
        public decimal? RP2 { get; set; }

        public decimal? P3 { get; set; }
        public decimal? RP3 { get; set; }

        public decimal? P4 { get; set; }
        public decimal? RP4 { get; set; }

        public decimal? Final { get; set; }
    }
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

    [HttpGet("secundaria/curso/{idCurso}/periodo/{idPeriodoPublicacion}")]
    public async Task<IActionResult> ReporteSecundariaPorCursoPeriodo(
    int idCurso,
    int idPeriodoPublicacion)
    {
        var curso = await _context.Cursos
            .Include(c => c.Grado)
                .ThenInclude(g => g.Nivel)
            .FirstOrDefaultAsync(c => c.IdCurso == idCurso);

        if (curso == null)
            return NotFound("Curso no encontrado.");

        if (curso.Grado.Nivel.Nombre != "Secundaria")
            return BadRequest("Este reporte solo aplica para secundaria.");

        var periodo = await _context.PeriodosPublicacion
            .FirstOrDefaultAsync(p => p.IdPeriodoPublicacion == idPeriodoPublicacion);

        if (periodo == null)
            return NotFound("Período no encontrado.");

        var estudiantes = await _context.Inscripciones
            .Include(i => i.Estudiante)
            .Where(i => i.IdCurso == idCurso && i.Estado == "Activo")
            .Select(i => i.Estudiante)
            .OrderBy(e => e.Apellidos)
            .ThenBy(e => e.Nombres)
            .ToListAsync();

        var reportes = new List<object>();

        foreach (var estudiante in estudiantes)
        {
            var calificaciones = await _context.CalificacionesCompetenciasPeriodo
                .Include(c => c.AsignacionDocente)
                    .ThenInclude(a => a.Materia)
                .Include(c => c.Competencia)
                .Where(c =>
                    c.IdEstudiante == estudiante.IdEstudiante &&
                    c.IdPeriodoPublicacion == idPeriodoPublicacion &&
                    c.Publicada &&
                    c.AsignacionDocente.IdCurso == idCurso)
                .ToListAsync();

            var materias = calificaciones
                .GroupBy(c => new
                {
                    c.IdAsignacionDocente,
                    Materia = c.AsignacionDocente.Materia.Nombre
                })
                .Select(g => new
                {
                    g.Key.Materia,
                    C1 = g.Where(x => x.Competencia.Codigo == "C1").Select(x => (decimal?)x.Promedio).FirstOrDefault(),
                    C2 = g.Where(x => x.Competencia.Codigo == "C2").Select(x => (decimal?)x.Promedio).FirstOrDefault(),
                    C3 = g.Where(x => x.Competencia.Codigo == "C3").Select(x => (decimal?)x.Promedio).FirstOrDefault(),
                    C4 = g.Where(x => x.Competencia.Codigo == "C4").Select(x => (decimal?)x.Promedio).FirstOrDefault(),
                    PromedioFinal = g.Any()
                        ? Math.Round(g.Average(x => x.Promedio), 2)
                        : 0
                })
                .OrderBy(x => x.Materia)
                .ToList();

            reportes.Add(new
            {
                estudiante.IdEstudiante,
                Estudiante = estudiante.Nombres + " " + estudiante.Apellidos,
                estudiante.Matricula,
                Curso = curso.Nombre,
                Grado = curso.Grado.Nombre,
                Periodo = periodo.Nombre,
                Materias = materias
            });
        }

        return Ok(new
        {
            Curso = curso.Nombre,
            Grado = curso.Grado.Nombre,
            Periodo = periodo.Nombre,
            Reportes = reportes
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

    [HttpGet("secundaria/anual/curso/{idCurso}")]
    public async Task<IActionResult> ReporteAnualSecundariaPorCurso(int idCurso)
    {
        var curso = await _context.Cursos
            .Include(c => c.Grado)
                .ThenInclude(g => g.Nivel)
            .FirstOrDefaultAsync(c => c.IdCurso == idCurso);

        if (curso == null)
            return NotFound("Curso no encontrado.");

        if (curso.Grado.Nivel.Nombre != "Secundaria")
            return BadRequest("Este reporte solo aplica para secundaria.");

        var estudiantes = await _context.Inscripciones
            .Include(i => i.Estudiante)
            .Where(i =>
                i.IdCurso == idCurso &&
                i.Estado == "Activo")
            .Select(i => i.Estudiante)
            .OrderBy(e => e.Apellidos)
            .ThenBy(e => e.Nombres)
            .ToListAsync();

        var reportes = new List<object>();

        foreach (var estudiante in estudiantes)
        {
            var calificaciones = await _context.CalificacionesCompetenciasPeriodo
                .Include(c => c.AsignacionDocente)
                    .ThenInclude(a => a.Materia)
                .Include(c => c.Competencia)
                .Include(c => c.PeriodoPublicacion)
                .Where(c =>
                    c.IdEstudiante == estudiante.IdEstudiante &&
                    c.AsignacionDocente.IdCurso == idCurso &&
                    c.Publicada)
                .ToListAsync();

            var materias = calificaciones
                .GroupBy(c => new
                {
                    c.IdAsignacionDocente,
                    Materia = c.AsignacionDocente.Materia.Nombre
                })
                .Select(g =>
                {
                    decimal? pc1 = PromedioCompetencia(g.ToList(), "C1");
                    decimal? pc2 = PromedioCompetencia(g.ToList(), "C2");
                    decimal? pc3 = PromedioCompetencia(g.ToList(), "C3");
                    decimal? pc4 = PromedioCompetencia(g.ToList(), "C4");

                    var valores = new List<decimal?> { pc1, pc2, pc3, pc4 }
                        .Where(x => x.HasValue)
                        .Select(x => x!.Value)
                        .ToList();

                    var finalArea = valores.Any()
                        ? Math.Round(valores.Average(), 2)
                        : 0;

                    return new
                    {
                        g.Key.Materia,
                        PC1 = pc1,
                        PC2 = pc2,
                        PC3 = pc3,
                        PC4 = pc4,
                        FinalArea = finalArea
                    };
                })
                .OrderBy(x => x.Materia)
                .ToList();

            reportes.Add(new
            {
                estudiante.IdEstudiante,
                Estudiante = estudiante.Nombres + " " + estudiante.Apellidos,
                estudiante.Matricula,
                Curso = curso.Nombre,
                Grado = curso.Grado.Nombre,
                Materias = materias
            });
        }

        return Ok(new
        {
            Curso = curso.Nombre,
            Grado = curso.Grado.Nombre,
            Reportes = reportes
        });
    }

    private decimal? PromedioCompetencia(
        List<CalificacionCompetenciaPeriodo> calificaciones,
        string codigoCompetencia)
    {
        var notas = calificaciones
            .Where(c => c.Competencia.Codigo == codigoCompetencia)
            .Select(c => c.Promedio)
            .ToList();

        if (!notas.Any())
            return null;

        return Math.Round(notas.Average(), 2);
    }

    [HttpGet("primaria/curso/{idCurso}/periodo/{idPeriodoPublicacion}")]
    public async Task<IActionResult> ReportePrimariaPorCursoPeriodo(
    int idCurso,
    int idPeriodoPublicacion)
    {
        var curso = await _context.Cursos
            .Include(c => c.Grado)
                .ThenInclude(g => g.Nivel)
            .FirstOrDefaultAsync(c => c.IdCurso == idCurso);

        if (curso == null)
            return NotFound("Curso no encontrado.");

        if (!curso.Grado.Nivel.UsaCompetencias)
            return BadRequest("Este reporte solo aplica para primaria.");

        var periodo = await _context.PeriodosPublicacion
            .FirstOrDefaultAsync(p => p.IdPeriodoPublicacion == idPeriodoPublicacion);

        if (periodo == null)
            return NotFound("Período no encontrado.");

        var estudiantes = await _context.Inscripciones
            .Include(i => i.Estudiante)
            .Where(i => i.IdCurso == idCurso && i.Estado == "Activo")
            .Select(i => i.Estudiante)
            .OrderBy(e => e.Apellidos)
            .ThenBy(e => e.Nombres)
            .ToListAsync();

        var reportes = new List<object>();

        foreach (var estudiante in estudiantes)
        {
            var calificaciones = await _context.CalificacionesCompetenciasPeriodo
                .Include(c => c.AsignacionDocente)
                    .ThenInclude(a => a.Materia)
                .Include(c => c.Competencia)
                .Where(c =>
                    c.IdEstudiante == estudiante.IdEstudiante &&
                    c.IdPeriodoPublicacion == idPeriodoPublicacion &&
                    c.Publicada &&
                    c.AsignacionDocente.IdCurso == idCurso)
                .ToListAsync();

            var materias = calificaciones
                .GroupBy(c => new
                {
                    c.IdAsignacionDocente,
                    Materia = c.AsignacionDocente.Materia.Nombre
                })
                .Select(g => new
                {
                    g.Key.Materia,
                    C1 = g.Where(x => x.Competencia.Codigo == "C1")
                          .Select(x => (decimal?)x.Promedio)
                          .FirstOrDefault(),
                    C2 = g.Where(x => x.Competencia.Codigo == "C2")
                          .Select(x => (decimal?)x.Promedio)
                          .FirstOrDefault(),
                    C3 = g.Where(x => x.Competencia.Codigo == "C3")
                          .Select(x => (decimal?)x.Promedio)
                          .FirstOrDefault(),
                    PromedioFinal = g.Any()
                        ? Math.Round(g.Average(x => x.Promedio), 2)
                        : 0
                })
                .OrderBy(x => x.Materia)
                .ToList();

            reportes.Add(new
            {
                estudiante.IdEstudiante,
                Estudiante = estudiante.Nombres + " " + estudiante.Apellidos,
                estudiante.Matricula,
                Curso = curso.Nombre,
                Grado = curso.Grado.Nombre,
                Periodo = periodo.Nombre,
                Materias = materias
            });
        }

        return Ok(new
        {
            Curso = curso.Nombre,
            Grado = curso.Grado.Nombre,
            Periodo = periodo.Nombre,
            Reportes = reportes
        });
    }

    [HttpGet("maestros-pendientes")]
    public async Task<IActionResult> MaestrosPendientes(int idPeriodoPublicacion)
    {
        var periodo = await _context.PeriodosPublicacion.FindAsync(idPeriodoPublicacion);

        if (periodo == null)
            return NotFound("Período no encontrado.");

        var nivelCoordinador = NivelHelper.ObtenerNivelPorRol(User);

        var query = _context.AsignacionesDocentes
            .Include(a => a.Maestro)
            .Include(a => a.Curso)
                .ThenInclude(c => c.Grado)
                    .ThenInclude(g => g.Nivel)
            .Include(a => a.Materia)
            .Where(a => a.IdAnioEscolar == periodo.IdAnioEscolar && a.Activo)
            .AsQueryable();

        if (nivelCoordinador != null)
        {
            query = query.Where(a => a.Curso.Grado.Nivel.Nombre == nivelCoordinador);
        }

        var asignaciones = await query.ToListAsync();

        var resultado = new List<object>();

        foreach (var asignacion in asignaciones)
        {
            var totalEstudiantes = await _context.Inscripciones
                .CountAsync(i =>
                    i.IdCurso == asignacion.IdCurso &&
                    i.IdAnioEscolar == asignacion.IdAnioEscolar &&
                    i.Estado == "Activo");

            var totalPublicadas = await _context.CalificacionesCompetenciasPeriodo
                .CountAsync(c =>
                    c.IdAsignacionDocente == asignacion.IdAsignacionDocente &&
                    c.IdPeriodoPublicacion == idPeriodoPublicacion &&
                    c.Publicada);

            var completo = totalEstudiantes > 0 && totalPublicadas >= totalEstudiantes;

            resultado.Add(new
            {
                Maestro = asignacion.Maestro.Nombres + " " + asignacion.Maestro.Apellidos,
                Curso = asignacion.Curso.Nombre,
                Grado = asignacion.Curso.Grado.Nombre,
                Nivel = asignacion.Curso.Grado.Nivel.Nombre,
                Materia = asignacion.Materia.Nombre,
                TotalEstudiantes = totalEstudiantes,
                TotalPublicadas = totalPublicadas,
                Estado = completo ? "Completado" : "Pendiente"
            });
        }

        var completadas = resultado.Count(r =>
            r.GetType().GetProperty("Estado")?.GetValue(r)?.ToString() == "Completado");

        var pendientes = resultado.Count(r =>
            r.GetType().GetProperty("Estado")?.GetValue(r)?.ToString() == "Pendiente");

        return Ok(new
        {
            Periodo = periodo.Nombre,
            Nivel = nivelCoordinador ?? "Todos",
            TotalAsignaciones = resultado.Count,
            Completadas = completadas,
            Pendientes = pendientes,
            Detalle = resultado
        });
    }


    [HttpGet("primaria/anual/curso/{idCurso}")]
    public async Task<IActionResult> ReporteAnualPrimariaPorCurso(int idCurso)
    {
        var curso = await _context.Cursos
            .Include(c => c.Grado)
                .ThenInclude(g => g.Nivel)
            .FirstOrDefaultAsync(c => c.IdCurso == idCurso);

        if (curso == null)
            return NotFound("Curso no encontrado.");

        if (!curso.Grado.Nivel.UsaCompetencias)
            return BadRequest("Este reporte solo aplica para primaria.");

        var estudiantes = await _context.Inscripciones
            .Include(i => i.Estudiante)
            .Where(i => i.IdCurso == idCurso && i.Estado == "Activo")
            .Select(i => i.Estudiante)
            .OrderBy(e => e.Apellidos)
            .ThenBy(e => e.Nombres)
            .ToListAsync();

        var reportes = new List<object>();

        foreach (var estudiante in estudiantes)
        {
            var calificaciones = await _context.CalificacionesCompetenciasPeriodo
                .Include(c => c.AsignacionDocente)
                    .ThenInclude(a => a.Materia)
                .Include(c => c.Competencia)
                .Include(c => c.PeriodoPublicacion)
                .Where(c =>
                    c.IdEstudiante == estudiante.IdEstudiante &&
                    c.AsignacionDocente.IdCurso == idCurso &&
                    c.Publicada)
                .ToListAsync();

            var materias = calificaciones
                .GroupBy(c => new
                {
                    c.IdAsignacionDocente,
                    Materia = c.AsignacionDocente.Materia.Nombre
                })
                .Select(g =>
                {
                    var lista = g.ToList();

                    var c1 = ConstruirCompetenciaAnual(lista, "C1");
                    var c2 = ConstruirCompetenciaAnual(lista, "C2");
                    var c3 = ConstruirCompetenciaAnual(lista, "C3");

                    var finales = new List<decimal?>
                    {
                    c1.Final,
                    c2.Final,
                    c3.Final
                    }
                    .Where(x => x.HasValue)
                    .Select(x => x!.Value)
                    .ToList();

                    var finalArea = finales.Any()
                        ? Math.Round(finales.Average(), 2)
                        : 0;

                    return new
                    {
                        g.Key.Materia,
                        C1 = c1,
                        C2 = c2,
                        C3 = c3,
                        FinalArea = finalArea
                    };
                })
                .OrderBy(x => x.Materia)
                .ToList();

            reportes.Add(new
            {
                estudiante.IdEstudiante,
                Estudiante = estudiante.Nombres + " " + estudiante.Apellidos,
                estudiante.Matricula,
                Curso = curso.Nombre,
                Grado = curso.Grado.Nombre,
                Materias = materias
            });
        }

        return Ok(new
        {
            Curso = curso.Nombre,
            Grado = curso.Grado.Nombre,
            Reportes = reportes
        });
    }


    [HttpGet("politecnico/curso/{idCurso}/periodo/{idPeriodoPublicacion}")]
    public async Task<IActionResult> ReportePolitecnicoCursoPeriodo(
    int idCurso,
    int idPeriodoPublicacion)
    {
        var curso = await _context.Cursos
            .Include(c => c.Grado)
                .ThenInclude(g => g.Nivel)
            .FirstOrDefaultAsync(c => c.IdCurso == idCurso);

        if (curso == null)
            return NotFound("Curso no encontrado.");

        var periodo = await _context.PeriodosPublicacion
            .FirstOrDefaultAsync(p => p.IdPeriodoPublicacion == idPeriodoPublicacion);

        if (periodo == null)
            return NotFound("Período no encontrado.");

        var estudiantes = await _context.Inscripciones
            .Include(i => i.Estudiante)
            .Where(i =>
                i.IdCurso == idCurso &&
                i.Estado == "Activo")
            .Select(i => i.Estudiante)
            .OrderBy(e => e.Nombres)
            .ToListAsync();

        var asignaciones = await _context.AsignacionesDocentes
            .Include(a => a.Materia)
            .Where(a =>
                a.IdCurso == idCurso &&
                a.Activo)
            .ToListAsync();

        var asignacionesNormales = asignaciones
            .Where(a => !a.Materia.EsTecnica)
            .ToList();

        var asignacionesTecnicas = asignaciones
            .Where(a => a.Materia.EsTecnica)
            .ToList();

        var reportes = new List<object>();

        foreach (var estudiante in estudiantes)
        {
            var materiasNormales = new List<object>();

            foreach (var asignacion in asignacionesNormales)
            {
                var calificaciones = await _context.CalificacionesCompetenciasPeriodo
                    .Include(c => c.Competencia)
                    .Where(c =>
                        c.IdEstudiante == estudiante.IdEstudiante &&
                        c.IdAsignacionDocente == asignacion.IdAsignacionDocente &&
                        c.IdPeriodoPublicacion == idPeriodoPublicacion &&
                        c.Publicada)
                    .ToListAsync();

                decimal? c1 = calificaciones
                    .FirstOrDefault(c => c.Competencia.Codigo == "C1")
                    ?.Promedio;

                decimal? c2 = calificaciones
                    .FirstOrDefault(c => c.Competencia.Codigo == "C2")
                    ?.Promedio;

                decimal? c3 = calificaciones
                    .FirstOrDefault(c => c.Competencia.Codigo == "C3")
                    ?.Promedio;

                decimal? c4 = calificaciones
                    .FirstOrDefault(c => c.Competencia.Codigo == "C4")
                    ?.Promedio;

                var valores = new List<decimal?> { c1, c2, c3, c4 }
                    .Where(x => x.HasValue)
                    .Select(x => x!.Value)
                    .ToList();

                decimal? promedioFinal = valores.Any()
                    ? Math.Round(valores.Average(), 2)
                    : null;

                materiasNormales.Add(new
                {
                    Materia = asignacion.Materia.Nombre,
                    C1 = c1,
                    C2 = c2,
                    C3 = c3,
                    C4 = c4,
                    PromedioFinal = promedioFinal
                });
            }

            var materiasTecnicas = new List<object>();

            foreach (var asignacion in asignacionesTecnicas)
            {
                var ras = await _context.ResultadosAprendizaje
                    .Where(r =>
                        r.IdAsignacionDocente == asignacion.IdAsignacionDocente &&
                        r.Activo)
                    .OrderBy(r => r.IdResultadoAprendizaje)
                    .Take(12)
                    .ToListAsync();

                var valoresRA = new decimal?[12];

                for (int i = 0; i < ras.Count && i < 12; i++)
                {
                    var ra = ras[i];

                    var actividades = await _context.ActividadesCompetencias
                        .Where(a =>
                            a.IdResultadoAprendizaje == ra.IdResultadoAprendizaje &&
                            a.Activa)
                        .Select(a => a.IdActividadCompetencia)
                        .ToListAsync();

                    var notas = await _context.NotasCompetencias
                        .Where(n =>
                            actividades.Contains(n.IdActividadCompetencia) &&
                            n.IdEstudiante == estudiante.IdEstudiante)
                        .Select(n => n.Nota)
                        .ToListAsync();

                    if (notas.Any())
                    {
                        var notaRA = notas.Sum();

                        if (notaRA > ra.ValorMaximo)
                            notaRA = ra.ValorMaximo;

                        valoresRA[i] = Math.Round(notaRA, 2);
                    }
                }

                decimal? totalTecnico = valoresRA
     .Where(x => x.HasValue)
     .Select(x => x!.Value)
     .Sum();

                materiasTecnicas.Add(new
                {
                    Materia = asignacion.Materia.Nombre,
                    RA1 = valoresRA[0],
                    RA2 = valoresRA[1],
                    RA3 = valoresRA[2],
                    RA4 = valoresRA[3],
                    RA5 = valoresRA[4],
                    RA6 = valoresRA[5],
                    RA7 = valoresRA[6],
                    RA8 = valoresRA[7],
                    RA9 = valoresRA[8],
                    RA10 = valoresRA[9],
                    RA11 = valoresRA[10],
                    RA12 = valoresRA[11],
                    Total = totalTecnico
                });
            }

            reportes.Add(new
            {
                estudiante.IdEstudiante,
                Estudiante = estudiante.Nombres + " " + estudiante.Apellidos,
                MateriasNormales = materiasNormales,
                MateriasTecnicas = materiasTecnicas
            });
        }

        return Ok(new
        {
            Curso = curso.Nombre,
            Grado = curso.Grado.Nombre,
            Nivel = curso.Grado.Nivel.Nombre,
            Periodo = periodo.Nombre,
            Reportes = reportes
        });
    }


    [HttpGet("secundaria/anual/curso/{idCurso}/pdf")]
    public async Task<IActionResult> ReporteAnualSecundariaPdf(int idCurso)
    {
        var datos = await GenerarDatosReporteAnualSecundaria(idCurso);

        if (datos == null)
            return NotFound("No se encontraron datos para el reporte.");

        var templatePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "Templates",
            "Secundaria",
            "3ro.pdf"
        );

        if (!System.IO.File.Exists(templatePath))
            return NotFound("No se encontró la plantilla PDF de 3ro.");

        using var output = new MemoryStream();
        using var writer = new PdfWriter(output);
        using var pdfFinal = new PdfDocument(writer);

        var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        var fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

        foreach (var reporte in datos.Reportes)
        {
            using var reader = new PdfReader(templatePath);
            using var templatePdf = new PdfDocument(reader);

            templatePdf.CopyPagesTo(1, 1, pdfFinal);

            var page = pdfFinal.GetLastPage();
            var canvas = new PdfCanvas(page);

            // Nombre estudiante
            Escribir(canvas, fontBold, "PRUEBA TEXTO", 100, 550, 20);
            Escribir(canvas, fontBold, reporte.Estudiante, 250, 500, 10);

            // Datos materias
         

            float y = 382;

            foreach (var materia in reporte.Materias)
            {
                Escribir(canvas, font, FormatoNota(materia.PC1), 205, y, 9);
                Escribir(canvas, font, FormatoNota(materia.PC2), 240, y, 9);
                Escribir(canvas, font, FormatoNota(materia.PC3), 275, y, 9);
                Escribir(canvas, font, FormatoNota(materia.PC4), 310, y, 9);
                Escribir(canvas, fontBold, FormatoNota(materia.FinalArea), 350, y, 9);

                y -= 20;
            }
        }

        pdfFinal.Close();

        return File(
            output.ToArray(),
            "application/pdf",
            $"Reporte_Anual_Secundaria_3ro_Curso_{idCurso}.pdf"
        );
    }

    [HttpGet("politecnico/anual/curso/{idCurso}")]
    public async Task<IActionResult> ReporteAnualPolitecnicoPorCurso(int idCurso)
    {
        var curso = await _context.Cursos
            .Include(c => c.Grado)
                .ThenInclude(g => g.Nivel)
            .FirstOrDefaultAsync(c => c.IdCurso == idCurso);

        if (curso == null)
            return NotFound("Curso no encontrado.");

        var estudiantes = await _context.Inscripciones
            .Include(i => i.Estudiante)
            .Where(i => i.IdCurso == idCurso && i.Estado == "Activo")
            .Select(i => i.Estudiante)
            .OrderBy(e => e.Apellidos)
            .ThenBy(e => e.Nombres)
            .ToListAsync();

        var asignaciones = await _context.AsignacionesDocentes
            .Include(a => a.Materia)
            .Where(a => a.IdCurso == idCurso && a.Activo)
            .ToListAsync();

        var asignacionesNormales = asignaciones
            .Where(a => !a.Materia.EsTecnica)
            .ToList();

        var asignacionesTecnicas = asignaciones
            .Where(a => a.Materia.EsTecnica)
            .ToList();

        var reportes = new List<object>();

        foreach (var estudiante in estudiantes)
        {
            var materiasNormales = new List<object>();

            foreach (var asignacion in asignacionesNormales)
            {
                var calificaciones = await _context.CalificacionesCompetenciasPeriodo
                    .Include(c => c.Competencia)
                    .Where(c =>
                        c.IdEstudiante == estudiante.IdEstudiante &&
                        c.IdAsignacionDocente == asignacion.IdAsignacionDocente &&
                        c.Publicada)
                    .ToListAsync();

                decimal? pc1 = PromedioCompetencia(calificaciones, "C1");
                decimal? pc2 = PromedioCompetencia(calificaciones, "C2");
                decimal? pc3 = PromedioCompetencia(calificaciones, "C3");
                decimal? pc4 = PromedioCompetencia(calificaciones, "C4");

                var valores = new List<decimal?> { pc1, pc2, pc3, pc4 }
                    .Where(x => x.HasValue)
                    .Select(x => x!.Value)
                    .ToList();

                var finalArea = valores.Any()
                    ? Math.Round(valores.Average(), 2)
                    : 0;

                materiasNormales.Add(new
                {
                    Materia = asignacion.Materia.Nombre,
                    PC1 = pc1,
                    PC2 = pc2,
                    PC3 = pc3,
                    PC4 = pc4,
                    FinalArea = finalArea
                });
            }

            var materiasTecnicas = new List<object>();

            foreach (var asignacion in asignacionesTecnicas)
            {
                var ras = await _context.ResultadosAprendizaje
                    .Where(r =>
                        r.IdAsignacionDocente == asignacion.IdAsignacionDocente &&
                        r.Activo)
                    .OrderBy(r => r.Codigo)
                    .Take(12)
                    .ToListAsync();

                var valoresRA = new decimal?[12];

                for (int i = 0; i < ras.Count && i < 12; i++)
                {
                    var ra = ras[i];

                    var actividadesIds = await _context.ActividadesCompetencias
                        .Where(a =>
                            a.IdResultadoAprendizaje == ra.IdResultadoAprendizaje &&
                            a.Activa)
                        .Select(a => a.IdActividadCompetencia)
                        .ToListAsync();

                    var notaRA = await _context.NotasCompetencias
                        .Where(n =>
                            actividadesIds.Contains(n.IdActividadCompetencia) &&
                            n.IdEstudiante == estudiante.IdEstudiante)
                        .SumAsync(n => (decimal?)n.Nota) ?? 0;

                    if (notaRA > ra.ValorMaximo)
                        notaRA = ra.ValorMaximo;

                    if (notaRA > 0)
                        valoresRA[i] = Math.Round(notaRA, 2);
                }

                var totalTecnico = valoresRA
                    .Where(x => x.HasValue)
                    .Select(x => x!.Value)
                    .Sum();

                materiasTecnicas.Add(new
                {
                    Materia = asignacion.Materia.Nombre,
                    RA1 = valoresRA[0],
                    RA2 = valoresRA[1],
                    RA3 = valoresRA[2],
                    RA4 = valoresRA[3],
                    RA5 = valoresRA[4],
                    RA6 = valoresRA[5],
                    RA7 = valoresRA[6],
                    RA8 = valoresRA[7],
                    RA9 = valoresRA[8],
                    RA10 = valoresRA[9],
                    RA11 = valoresRA[10],
                    RA12 = valoresRA[11],
                    Total = totalTecnico
                });
            }

            reportes.Add(new
            {
                estudiante.IdEstudiante,
                Estudiante = estudiante.Nombres + " " + estudiante.Apellidos,
                estudiante.Matricula,
                Curso = curso.Nombre,
                Grado = curso.Grado.Nombre,
                MateriasNormales = materiasNormales,
                MateriasTecnicas = materiasTecnicas
            });
        }

        return Ok(new
        {
            Curso = curso.Nombre,
            Grado = curso.Grado.Nombre,
            Nivel = curso.Grado.Nivel.Nombre,
            Reportes = reportes
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
    private CompetenciaAnualDto ConstruirCompetenciaAnual(
    List<CalificacionCompetenciaPeriodo> calificaciones,
    string codigoCompetencia)
    {
        var notas = calificaciones
            .Where(c => c.Competencia.Codigo == codigoCompetencia)
            .Select(c => new
            {
                Periodo = c.PeriodoPublicacion.Nombre,
                c.Promedio
            })
            .ToList();

        decimal? p1 = notas.FirstOrDefault(n => n.Periodo.ToLower().Contains("primer"))?.Promedio;
        decimal? p2 = notas.FirstOrDefault(n => n.Periodo.ToLower().Contains("segundo"))?.Promedio;
        decimal? p3 = notas.FirstOrDefault(n => n.Periodo.ToLower().Contains("tercer"))?.Promedio;
        decimal? p4 = notas.FirstOrDefault(n => n.Periodo.ToLower().Contains("cuarto"))?.Promedio;

        var valores = new List<decimal?> { p1, p2, p3, p4 }
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .ToList();

        decimal? final = valores.Any()
            ? Math.Round(valores.Average(), 2)
            : null;
        return new CompetenciaAnualDto
        {
            P1 = p1,
            RP1 = null,

            P2 = p2,
            RP2 = null,

            P3 = p3,
            RP3 = null,

            P4 = p4,
            RP4 = null,

            Final = final
        };
    }

    private static void Escribir(
    PdfCanvas canvas,
    PdfFont font,
    string texto,
    float x,
    float y,
    float size)
    {
        canvas.BeginText();
        canvas.SetFontAndSize(font, size);
        canvas.MoveText(x, y);
        canvas.ShowText(texto ?? "");
        canvas.EndText();
    }

    private static string FormatoNota(decimal? nota)
    {
        if (!nota.HasValue)
            return "";

        return Math.Round(nota.Value, 0).ToString();
    }


    private class ReporteAnualSecundariaData
    {
        public string Curso { get; set; } = "";
        public string Grado { get; set; } = "";
        public List<ReporteAnualEstudianteData> Reportes { get; set; } = new();
    }

    private class ReporteAnualEstudianteData
    {
        public int IdEstudiante { get; set; }
        public string Estudiante { get; set; } = "";
        public string Matricula { get; set; } = "";
        public List<ReporteAnualMateriaData> Materias { get; set; } = new();
    }

    private class ReporteAnualMateriaData
    {
        public string Materia { get; set; } = "";
        public decimal? PC1 { get; set; }
        public decimal? PC2 { get; set; }
        public decimal? PC3 { get; set; }
        public decimal? PC4 { get; set; }
        public decimal FinalArea { get; set; }
    }


    private async Task<ReporteAnualSecundariaData?> GenerarDatosReporteAnualSecundaria(int idCurso)
    {
        var curso = await _context.Cursos
            .Include(c => c.Grado)
                .ThenInclude(g => g.Nivel)
            .FirstOrDefaultAsync(c => c.IdCurso == idCurso);

        if (curso == null)
            return null;

        var estudiantes = await _context.Inscripciones
            .Include(i => i.Estudiante)
            .Where(i =>
                i.IdCurso == idCurso &&
                i.Estado == "Activo")
            .Select(i => i.Estudiante)
            .OrderBy(e => e.Apellidos)
            .ThenBy(e => e.Nombres)
            .ToListAsync();

        var resultado = new ReporteAnualSecundariaData
        {
            Curso = curso.Nombre,
            Grado = curso.Grado.Nombre
        };

        foreach (var estudiante in estudiantes)
        {
            var calificaciones = await _context.CalificacionesCompetenciasPeriodo
                .Include(c => c.AsignacionDocente)
                    .ThenInclude(a => a.Materia)
                .Include(c => c.Competencia)
                .Include(c => c.PeriodoPublicacion)
                .Where(c =>
                    c.IdEstudiante == estudiante.IdEstudiante &&
                    c.AsignacionDocente.IdCurso == idCurso &&
                    c.Publicada)
                .ToListAsync();

            var materias = calificaciones
                .GroupBy(c => new
                {
                    c.IdAsignacionDocente,
                    Materia = c.AsignacionDocente.Materia.Nombre
                })
                .Select(g =>
                {
                    decimal? pc1 = PromedioCompetencia(g.ToList(), "C1");
                    decimal? pc2 = PromedioCompetencia(g.ToList(), "C2");
                    decimal? pc3 = PromedioCompetencia(g.ToList(), "C3");
                    decimal? pc4 = PromedioCompetencia(g.ToList(), "C4");

                    var valores = new List<decimal?> { pc1, pc2, pc3, pc4 }
                        .Where(x => x.HasValue)
                        .Select(x => x!.Value)
                        .ToList();

                    var finalArea = valores.Any()
                        ? Math.Round(valores.Average(), 0)
                        : 0;

                    return new ReporteAnualMateriaData
                    {
                        Materia = g.Key.Materia,
                        PC1 = pc1,
                        PC2 = pc2,
                        PC3 = pc3,
                        PC4 = pc4,
                        FinalArea = finalArea
                    };
                })
                .OrderBy(x => x.Materia)
                .ToList();

            resultado.Reportes.Add(new ReporteAnualEstudianteData
            {
                IdEstudiante = estudiante.IdEstudiante,
                Estudiante = estudiante.Nombres + " " + estudiante.Apellidos,
                Matricula = estudiante.Matricula,
                Materias = materias
            });
        }

        return resultado;
    }



}