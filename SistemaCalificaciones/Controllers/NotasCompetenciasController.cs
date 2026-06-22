using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.DTOs.Competencias;
using SistemaCalificaciones.Models;

namespace SistemaCalificaciones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,Maestro,CoordinadorPrimaria,CoordinadorSecundaria,CoordinadorPolitecnico")]
public class NotasCompetenciasController : ControllerBase
{
    private readonly AppDbContext _context;

    public NotasCompetenciasController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("actividad/{idActividadCompetencia}")]
    public async Task<IActionResult> GetPorActividad(int idActividadCompetencia)
    {
        var notas = await _context.NotasCompetencias
            .Include(n => n.Estudiante)
            .Where(n => n.IdActividadCompetencia == idActividadCompetencia)
            .OrderBy(n => n.Estudiante.Nombres)
            .Select(n => new
            {
                n.IdNotaCompetencia,
                n.IdActividadCompetencia,
                n.IdEstudiante,
                Estudiante = n.Estudiante.Nombres + " " + n.Estudiante.Apellidos,
                n.Nota,
                n.FechaRegistro
            })
            .ToListAsync();

        return Ok(notas);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(CrearNotaCompetenciaDto dto)
    {
        if (dto.Nota < 0 || dto.Nota > 100)
            return BadRequest("La nota debe estar entre 0 y 100.");

        var actividad = await _context.ActividadesCompetencias
            .Include(a => a.PeriodoPublicacion)
            .FirstOrDefaultAsync(a => a.IdActividadCompetencia == dto.IdActividadCompetencia);

        if (actividad == null || !actividad.Activa)
            return BadRequest("La actividad no existe o está inactiva.");

        // Solo validar fecha de cierre si es actividad por competencias.
        // Las actividades RA son anuales y no tienen período.
        if (actividad.IdPeriodoPublicacion.HasValue &&
            actividad.PeriodoPublicacion != null &&
            DateTime.Now.Date > actividad.PeriodoPublicacion.FechaCierre.Date)
        {
            return BadRequest("El período ya cerró. No puedes registrar notas.");
        }

        var estudianteExiste = await _context.Estudiantes
            .AnyAsync(e => e.IdEstudiante == dto.IdEstudiante && e.Activo);

        if (!estudianteExiste)
            return BadRequest("El estudiante no existe o está inactivo.");

        var existeNota = await _context.NotasCompetencias
            .AnyAsync(n =>
                n.IdActividadCompetencia == dto.IdActividadCompetencia &&
                n.IdEstudiante == dto.IdEstudiante);

        if (existeNota)
            return BadRequest("Este estudiante ya tiene nota en esta actividad.");

        var nota = new NotaCompetencia
        {
            IdActividadCompetencia = dto.IdActividadCompetencia,
            IdEstudiante = dto.IdEstudiante,
            Nota = dto.Nota,
            FechaRegistro = DateTime.Now
        };

        _context.NotasCompetencias.Add(nota);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = actividad.IdResultadoAprendizaje.HasValue
                ? "Nota por RA registrada correctamente."
                : "Nota por competencia registrada correctamente.",
            nota.IdNotaCompetencia,
            nota.IdEstudiante,
            nota.Nota
        });
    }

    [HttpGet("asignacion/{idAsignacionDocente}/periodo/{idPeriodoPublicacion}")]
    public async Task<IActionResult> GetNotasPorAsignacionPeriodo(
    int idAsignacionDocente,
    int idPeriodoPublicacion)
    {
        var notas = await _context.NotasCompetencias
            .Include(n => n.ActividadCompetencia)
            .Where(n =>
                n.ActividadCompetencia.IdAsignacionDocente == idAsignacionDocente &&
                n.ActividadCompetencia.IdPeriodoPublicacion == idPeriodoPublicacion)
            .Select(n => new
            {
                n.IdNotaCompetencia,
                n.IdActividadCompetencia,
                n.IdEstudiante,
                n.Nota
            })
            .ToListAsync();

        return Ok(notas);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, CrearNotaCompetenciaDto dto)
    {
        if (dto.Nota < 0 || dto.Nota > 100)
            return BadRequest("La nota debe estar entre 0 y 100.");

        var nota = await _context.NotasCompetencias
            .Include(n => n.ActividadCompetencia)
                .ThenInclude(a => a.PeriodoPublicacion)
            .FirstOrDefaultAsync(n => n.IdNotaCompetencia == id);

        if (nota == null)
            return NotFound("Nota no encontrada.");

        if (nota.ActividadCompetencia.IdPeriodoPublicacion.HasValue &&
     nota.ActividadCompetencia.PeriodoPublicacion != null &&
     DateTime.Now.Date > nota.ActividadCompetencia.PeriodoPublicacion.FechaCierre.Date)
        {
            return BadRequest("El período ya cerró. No puedes modificar esta nota.");
        }

        nota.Nota = dto.Nota;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Nota actualizada correctamente.",
            nota.IdNotaCompetencia,
            nota.Nota
        });
    }


    public class GuardarNotaCompetenciaDto
    {
        public int IdActividadCompetencia { get; set; }
        public int IdEstudiante { get; set; }
        public decimal? Nota { get; set; }
    }

    [HttpPost("guardar-masivo")]
    public async Task<IActionResult> GuardarMasivo(
        List<GuardarNotaCompetenciaDto> notasDto)
    {
        foreach (var item in notasDto)
        {
            if (item.Nota == null)
                continue;

            if (item.Nota < 0 || item.Nota > 100)
                return BadRequest("Las notas deben estar entre 0 y 100.");

            var notaExistente = await _context.NotasCompetencias
                .FirstOrDefaultAsync(n =>
                    n.IdActividadCompetencia == item.IdActividadCompetencia &&
                    n.IdEstudiante == item.IdEstudiante);

            if (notaExistente == null)
            {
                var nuevaNota = new NotaCompetencia
                {
                    IdActividadCompetencia = item.IdActividadCompetencia,
                    IdEstudiante = item.IdEstudiante,
                    Nota = item.Nota.Value,
                    FechaRegistro = DateTime.Now
                };

                _context.NotasCompetencias.Add(nuevaNota);
            }
            else
            {
                notaExistente.Nota = item.Nota.Value;
                notaExistente.FechaRegistro = DateTime.Now;
            }
        }

        await _context.SaveChangesAsync();

        return Ok("Calificaciones guardadas correctamente.");
    }
}