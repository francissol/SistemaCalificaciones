using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.DTOs.Competencias;
using SistemaCalificaciones.Models;

namespace SistemaCalificaciones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,Maestro")]
public class ActividadesCompetenciasController : ControllerBase
{
    private readonly AppDbContext _context;

    public ActividadesCompetenciasController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("asignacion/{idAsignacionDocente}/periodo/{idPeriodoPublicacion}")]
    public async Task<IActionResult> GetPorAsignacionYPeriodo(
        int idAsignacionDocente,
        int idPeriodoPublicacion)
    {
        var actividades = await _context.ActividadesCompetencias
            .Include(a => a.Competencia)
            .Where(a =>
                a.IdAsignacionDocente == idAsignacionDocente &&
                a.IdPeriodoPublicacion == idPeriodoPublicacion &&
                a.Activa)
            .OrderBy(a => a.Competencia.Codigo)
            .ThenBy(a => a.Nombre)
            .Select(a => new
            {
                a.IdActividadCompetencia,
                a.IdAsignacionDocente,
                a.IdPeriodoPublicacion,
                a.IdCompetencia,
                Competencia = a.Competencia.Codigo + " - " + a.Competencia.Nombre,
                a.Nombre,
                a.FechaCreacion,
                a.Activa
            })
            .ToListAsync();

        return Ok(actividades);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(CrearActividadCompetenciaDto dto)
    {
        var asignacion = await _context.AsignacionesDocentes
            .Include(a => a.Curso)
                .ThenInclude(c => c.Grado)
                    .ThenInclude(g => g.Nivel)
            .Include(a => a.Materia)
            .FirstOrDefaultAsync(a => a.IdAsignacionDocente == dto.IdAsignacionDocente);

        if (asignacion == null || !asignacion.Activo)
            return BadRequest("La asignación docente no existe o está inactiva.");

        if (!asignacion.Curso.Grado.Nivel.UsaCompetencias)
            return BadRequest("Este curso no pertenece a un nivel que use competencias.");

        var periodo = await _context.PeriodosPublicacion
            .FirstOrDefaultAsync(p => p.IdPeriodoPublicacion == dto.IdPeriodoPublicacion);

        if (periodo == null || !periodo.Activo)
            return BadRequest("El período no existe o está inactivo.");

        if (DateTime.Now.Date > periodo.FechaCierre.Date)
            return BadRequest("Este período ya cerró. No puedes crear actividades.");

        var competenciaAsignada = await _context.CompetenciasGradoMateria
            .AnyAsync(cgm =>
                cgm.IdGrado == asignacion.Curso.IdGrado &&
                cgm.IdMateria == asignacion.IdMateria &&
                cgm.IdCompetencia == dto.IdCompetencia);

        if (!competenciaAsignada)
            return BadRequest("Esta competencia no está asignada a esa materia y grado.");

        var actividad = new ActividadCompetencia
        {
            IdAsignacionDocente = dto.IdAsignacionDocente,
            IdPeriodoPublicacion = dto.IdPeriodoPublicacion,
            IdCompetencia = dto.IdCompetencia,
            Nombre = dto.Nombre,
            FechaCreacion = DateTime.Now,
            Activa = true
        };

        _context.ActividadesCompetencias.Add(actividad);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Actividad por competencia creada correctamente.",
            actividad.IdActividadCompetencia,
            actividad.Nombre
        });
    }

    [HttpPut("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        var actividad = await _context.ActividadesCompetencias.FindAsync(id);

        if (actividad == null)
            return NotFound("Actividad no encontrada.");

        actividad.Activa = !actividad.Activa;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Estado actualizado correctamente.",
            actividad.IdActividadCompetencia,
            actividad.Activa
        });
    }
}