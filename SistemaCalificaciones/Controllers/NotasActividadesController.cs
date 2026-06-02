using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.DTOs.NotasActividades;
using SistemaCalificaciones.Models;

namespace SistemaCalificaciones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,Maestro")]
public class NotasActividadesController : ControllerBase
{
    private readonly AppDbContext _context;

    public NotasActividadesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("actividad/{idActividadEvaluativa}")]
    public async Task<IActionResult> GetPorActividad(int idActividadEvaluativa)
    {
        var notas = await _context.NotasActividades
            .Include(n => n.Estudiante)
            .Where(n => n.IdActividadEvaluativa == idActividadEvaluativa)
            .OrderBy(n => n.Estudiante.Nombres)
            .Select(n => new
            {
                n.IdNotaActividad,
                n.IdActividadEvaluativa,
                n.IdEstudiante,
                Estudiante = n.Estudiante.Nombres + " " + n.Estudiante.Apellidos,
                n.Nota,
                n.FechaRegistro,
                n.FechaUltimaModificacion
            })
            .ToListAsync();

        return Ok(notas);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(CrearNotaActividadDto dto)
    {
        if (dto.Nota < 0 || dto.Nota > 100)
            return BadRequest("La nota debe estar entre 0 y 100.");

        var actividad = await _context.ActividadesEvaluativas
            .Include(a => a.PeriodoPublicacion)
            .FirstOrDefaultAsync(a => a.IdActividadEvaluativa == dto.IdActividadEvaluativa);

        if (actividad == null || !actividad.Activa)
            return BadRequest("La actividad no existe o está inactiva.");

        if (DateTime.Now.Date > actividad.PeriodoPublicacion.FechaCierre.Date)
            return BadRequest("El período ya cerró. No puedes registrar notas.");

        var estudianteExiste = await _context.Estudiantes
            .AnyAsync(e => e.IdEstudiante == dto.IdEstudiante && e.Activo);

        if (!estudianteExiste)
            return BadRequest("El estudiante no existe o está inactivo.");

        var existeNota = await _context.NotasActividades
            .AnyAsync(n =>
                n.IdActividadEvaluativa == dto.IdActividadEvaluativa &&
                n.IdEstudiante == dto.IdEstudiante);

        if (existeNota)
            return BadRequest("Este estudiante ya tiene nota en esta actividad.");

        var nota = new NotaActividad
        {
            IdActividadEvaluativa = dto.IdActividadEvaluativa,
            IdEstudiante = dto.IdEstudiante,
            Nota = dto.Nota,
            FechaRegistro = DateTime.Now
        };

        _context.NotasActividades.Add(nota);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Nota registrada correctamente.",
            nota.IdNotaActividad,
            nota.IdEstudiante,
            nota.Nota
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, CrearNotaActividadDto dto)
    {
        if (dto.Nota < 0 || dto.Nota > 100)
            return BadRequest("La nota debe estar entre 0 y 100.");

        var nota = await _context.NotasActividades
            .Include(n => n.ActividadEvaluativa)
            .ThenInclude(a => a.PeriodoPublicacion)
            .FirstOrDefaultAsync(n => n.IdNotaActividad == id);

        if (nota == null)
            return NotFound("Nota no encontrada.");

        if (DateTime.Now.Date > nota.ActividadEvaluativa.PeriodoPublicacion.FechaCierre.Date)
            return BadRequest("El período ya cerró. No puedes modificar esta nota.");

        var diasDesdeRegistro = (DateTime.Now.Date - nota.FechaRegistro.Date).Days;

        if (diasDesdeRegistro > 10)
            return BadRequest("Esta nota ya no puede modificarse porque pasaron más de 10 días desde su registro.");

        nota.Nota = dto.Nota;
        nota.FechaUltimaModificacion = DateTime.Now;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Nota actualizada correctamente.",
            nota.IdNotaActividad,
            nota.Nota,
            nota.FechaUltimaModificacion
        });
    }
}