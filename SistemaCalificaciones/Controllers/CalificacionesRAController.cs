using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.Models;

namespace SistemaCalificaciones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,Maestro")]
public class CalificacionesRAController : ControllerBase
{
    private readonly AppDbContext _context;

    public CalificacionesRAController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("calcular")]
    public async Task<IActionResult> Calcular(int idAsignacionDocente)
    {
        var asignacion = await _context.AsignacionesDocentes
            .Include(a => a.Materia)
            .FirstOrDefaultAsync(a => a.IdAsignacionDocente == idAsignacionDocente);

        if (asignacion == null)
            return NotFound("Asignación no encontrada.");

        if (!asignacion.Materia.EsTecnica)
            return BadRequest("Esta asignación no pertenece a una materia técnica.");

        var ras = await _context.ResultadosAprendizaje
            .Where(r => r.IdAsignacionDocente == idAsignacionDocente && r.Activo)
            .OrderBy(r => r.Codigo)
            .ToListAsync();

        var totalRA = ras.Sum(r => r.ValorMaximo);

        if (totalRA != 100)
            return BadRequest($"La suma de los RA debe ser exactamente 100. Actualmente suma {totalRA}.");

        var estudiantes = await _context.Inscripciones
            .Include(i => i.Estudiante)
            .Where(i =>
                i.IdCurso == asignacion.IdCurso &&
                i.Estado == "Activo")
            .Select(i => i.Estudiante)
            .ToListAsync();

        foreach (var estudiante in estudiantes)
        {
            decimal notaFinal = 0;

            foreach (var ra in ras)
            {
                var actividadesIds = await _context.ActividadesCompetencias
                    .Where(a =>
                        a.IdResultadoAprendizaje == ra.IdResultadoAprendizaje &&
                        a.Activa)
                    .Select(a => a.IdActividadCompetencia)
                    .ToListAsync();

                if (!actividadesIds.Any())
                    continue;

                var notas = await _context.NotasCompetencias
                    .Where(n =>
                        actividadesIds.Contains(n.IdActividadCompetencia) &&
                        n.IdEstudiante == estudiante.IdEstudiante)
                    .Select(n => n.Nota)
                    .ToListAsync();

                if (!notas.Any())
                    continue;

                var promedioRA = notas.Average();
                var aporteRA = promedioRA * ra.ValorMaximo / 100;

                notaFinal += aporteRA;
            }

            notaFinal = Math.Round(notaFinal, 2);

            var existente = await _context.CalificacionesRA
                .FirstOrDefaultAsync(c =>
                    c.IdAsignacionDocente == idAsignacionDocente &&
                    c.IdEstudiante == estudiante.IdEstudiante);

            if (existente == null)
            {
                _context.CalificacionesRA.Add(new CalificacionRA
                {
                    IdAsignacionDocente = idAsignacionDocente,
                    IdEstudiante = estudiante.IdEstudiante,
                    NotaFinal = notaFinal,
                    Publicada = false,
                    FechaCalculo = DateTime.Now
                });
            }
            else
            {
                existente.NotaFinal = notaFinal;
                existente.FechaCalculo = DateTime.Now;
            }
        }

        await _context.SaveChangesAsync();

        return Ok("Calificaciones RA calculadas correctamente.");
    }

    [HttpPut("publicar")]
    public async Task<IActionResult> Publicar(int idAsignacionDocente)
    {
        var calificaciones = await _context.CalificacionesRA
            .Where(c => c.IdAsignacionDocente == idAsignacionDocente)
            .ToListAsync();

        if (!calificaciones.Any())
            return BadRequest("Primero debes calcular las calificaciones RA.");

        foreach (var c in calificaciones)
        {
            c.Publicada = true;
            c.FechaPublicacion = DateTime.Now;
        }

        await _context.SaveChangesAsync();

        return Ok("Calificaciones RA publicadas correctamente.");
    }
}