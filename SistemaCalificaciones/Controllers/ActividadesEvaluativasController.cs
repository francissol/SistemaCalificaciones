using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.DTOs.ActividadesEvaluativas;
using SistemaCalificaciones.Models;

namespace SistemaCalificaciones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ActividadesEvaluativasController : ControllerBase
{
    private readonly AppDbContext _context;

    public ActividadesEvaluativasController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("asignacion/{idAsignacionDocente}/periodo/{idPeriodoPublicacion}")]
    public async Task<IActionResult> GetPorAsignacionYPeriodo(int idAsignacionDocente, int idPeriodoPublicacion)
    {
        var actividades = await _context.ActividadesEvaluativas
            .Where(a =>
                a.IdAsignacionDocente == idAsignacionDocente &&
                a.IdPeriodoPublicacion == idPeriodoPublicacion)
            .OrderBy(a => a.Nombre)
            .Select(a => new
            {
                a.IdActividadEvaluativa,
                a.IdAsignacionDocente,
                a.IdPeriodoPublicacion,
                a.Nombre,
                a.Porcentaje,
                a.FechaCreacion,
                a.Activa
            })
            .ToListAsync();

        return Ok(actividades);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Maestro")]
    public async Task<IActionResult> Crear(CrearActividadEvaluativaDto dto)
    {
        var asignacion = await _context.AsignacionesDocentes
            .Include(a => a.Curso)
            .Include(a => a.Materia)
            .FirstOrDefaultAsync(a => a.IdAsignacionDocente == dto.IdAsignacionDocente);

        if (asignacion == null || !asignacion.Activo)
            return BadRequest("La asignación docente no existe o está inactiva.");

        var periodo = await _context.PeriodosPublicacion
            .FirstOrDefaultAsync(p => p.IdPeriodoPublicacion == dto.IdPeriodoPublicacion);

        if (periodo == null || !periodo.Activo)
            return BadRequest("El período no existe o está inactivo.");

        if (DateTime.Now.Date > periodo.FechaCierre.Date)
            return BadRequest("Este período ya cerró. No puedes crear actividades.");

        if (dto.Porcentaje <= 0 || dto.Porcentaje > 100)
            return BadRequest("El porcentaje debe ser mayor que 0 y menor o igual a 100.");

        var porcentajeActual = await _context.ActividadesEvaluativas
            .Where(a =>
                a.IdAsignacionDocente == dto.IdAsignacionDocente &&
                a.IdPeriodoPublicacion == dto.IdPeriodoPublicacion &&
                a.Activa)
            .SumAsync(a => a.Porcentaje);

        if (porcentajeActual + dto.Porcentaje > 100)
            return BadRequest($"No puedes exceder el 100%. Actualmente tienes {porcentajeActual}% registrado.");

        var existeNombre = await _context.ActividadesEvaluativas
            .AnyAsync(a =>
                a.IdAsignacionDocente == dto.IdAsignacionDocente &&
                a.IdPeriodoPublicacion == dto.IdPeriodoPublicacion &&
                a.Nombre == dto.Nombre &&
                a.Activa);

        if (existeNombre)
            return BadRequest("Ya existe una actividad con ese nombre para esta materia y período.");

        var actividad = new ActividadEvaluativa
        {
            IdAsignacionDocente = dto.IdAsignacionDocente,
            IdPeriodoPublicacion = dto.IdPeriodoPublicacion,
            Nombre = dto.Nombre,
            Porcentaje = dto.Porcentaje,
            FechaCreacion = DateTime.Now,
            Activa = true
        };

        _context.ActividadesEvaluativas.Add(actividad);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Actividad creada correctamente.",
            actividad.IdActividadEvaluativa,
            actividad.Nombre,
            actividad.Porcentaje
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador,Maestro")]
    public async Task<IActionResult> Actualizar(int id, CrearActividadEvaluativaDto dto)
    {
        var actividad = await _context.ActividadesEvaluativas.FindAsync(id);

        if (actividad == null)
            return NotFound("Actividad no encontrada.");

        var periodo = await _context.PeriodosPublicacion
            .FirstOrDefaultAsync(p => p.IdPeriodoPublicacion == dto.IdPeriodoPublicacion);

        if (periodo == null)
            return BadRequest("El período no existe.");

        if (DateTime.Now.Date > periodo.FechaCierre.Date)
            return BadRequest("Este período ya cerró. No puedes modificar actividades.");

        var porcentajeActualSinEsta = await _context.ActividadesEvaluativas
            .Where(a =>
                a.IdAsignacionDocente == dto.IdAsignacionDocente &&
                a.IdPeriodoPublicacion == dto.IdPeriodoPublicacion &&
                a.IdActividadEvaluativa != id &&
                a.Activa)
            .SumAsync(a => a.Porcentaje);

        if (porcentajeActualSinEsta + dto.Porcentaje > 100)
            return BadRequest($"No puedes exceder el 100%. Actualmente tienes {porcentajeActualSinEsta}% sin contar esta actividad.");

        actividad.IdAsignacionDocente = dto.IdAsignacionDocente;
        actividad.IdPeriodoPublicacion = dto.IdPeriodoPublicacion;
        actividad.Nombre = dto.Nombre;
        actividad.Porcentaje = dto.Porcentaje;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Actividad actualizada correctamente.",
            actividad.IdActividadEvaluativa,
            actividad.Nombre,
            actividad.Porcentaje
        });
    }

    [HttpPut("{id}/estado")]
    [Authorize(Roles = "Administrador,Maestro")]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        var actividad = await _context.ActividadesEvaluativas.FindAsync(id);

        if (actividad == null)
            return NotFound("Actividad no encontrada.");

        actividad.Activa = !actividad.Activa;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Estado actualizado correctamente.",
            actividad.IdActividadEvaluativa,
            actividad.Activa
        });
    }
}