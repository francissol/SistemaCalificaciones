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

        if (DateTime.Now.Date > actividad.PeriodoPublicacion.FechaCierre.Date)
            return BadRequest("El período ya cerró. No puedes registrar notas.");

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
            mensaje = "Nota por competencia registrada correctamente.",
            nota.IdNotaCompetencia,
            nota.IdEstudiante,
            nota.Nota
        });
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

        if (DateTime.Now.Date > nota.ActividadCompetencia.PeriodoPublicacion.FechaCierre.Date)
            return BadRequest("El período ya cerró. No puedes modificar esta nota.");

        nota.Nota = dto.Nota;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Nota actualizada correctamente.",
            nota.IdNotaCompetencia,
            nota.Nota
        });
    }
}