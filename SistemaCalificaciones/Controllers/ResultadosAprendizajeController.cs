using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.Models;

namespace SistemaCalificaciones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Maestro,Administrador")]
public class ResultadosAprendizajeController : ControllerBase
{
    private readonly AppDbContext _context;

    public ResultadosAprendizajeController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("asignacion/{idAsignacion}")]
    public async Task<IActionResult> ObtenerRA(int idAsignacion)
    {
        var ras = await _context.ResultadosAprendizaje
            .Where(r =>
                r.IdAsignacionDocente == idAsignacion &&
                r.Activo)
            .OrderBy(r => r.Codigo)
            .Select(r => new
            {
                r.IdResultadoAprendizaje,
                r.IdAsignacionDocente,
                r.Codigo,
                r.Nombre,
                r.ValorMaximo,
                r.Activo
            })
            .ToListAsync();

        var total = ras.Sum(r => r.ValorMaximo);

        return Ok(new
        {
            total,
            faltante = 100 - total,
            completo = total == 100,
            resultados = ras
        });
    }

    [HttpPost("asignacion/{idAsignacion}")]
    public async Task<IActionResult> CrearRA(
        int idAsignacion,
        CrearRADto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest("El nombre del RA es obligatorio.");

        if (dto.ValorMaximo <= 0)
            return BadRequest("El valor máximo debe ser mayor que 0.");

        var asignacion = await _context.AsignacionesDocentes
            .Include(a => a.Materia)
            .FirstOrDefaultAsync(a => a.IdAsignacionDocente == idAsignacion);

        if (asignacion == null)
            return NotFound("Asignación no encontrada.");

        if (!asignacion.Materia.EsTecnica)
            return BadRequest("Esta materia no es técnica.");

        var totalActual = await _context.ResultadosAprendizaje
            .Where(r =>
                r.IdAsignacionDocente == idAsignacion &&
                r.Activo)
            .SumAsync(r => (decimal?)r.ValorMaximo) ?? 0;

        if (totalActual + dto.ValorMaximo > 100)
            return BadRequest($"La suma excede 100. Actualmente lleva {totalActual}.");

        var cantidad = await _context.ResultadosAprendizaje
            .CountAsync(r => r.IdAsignacionDocente == idAsignacion);

        var ra = new ResultadoAprendizaje
        {
            IdAsignacionDocente = idAsignacion,
            Codigo = $"RA{cantidad + 1}",
            Nombre = dto.Nombre,
            ValorMaximo = dto.ValorMaximo,
            Activo = true
        };

        _context.ResultadosAprendizaje.Add(ra);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            ra.IdResultadoAprendizaje,
            ra.IdAsignacionDocente,
            ra.Codigo,
            ra.Nombre,
            ra.ValorMaximo,
            ra.Activo
        });
    }

    [HttpPut("{idRA}")]
    public async Task<IActionResult> EditarRA(int idRA, CrearRADto dto)
    {
        var ra = await _context.ResultadosAprendizaje
            .FirstOrDefaultAsync(r => r.IdResultadoAprendizaje == idRA && r.Activo);

        if (ra == null)
            return NotFound("RA no encontrado.");

        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest("El nombre del RA es obligatorio.");

        if (dto.ValorMaximo <= 0)
            return BadRequest("El valor máximo debe ser mayor que 0.");

        var totalSinEste = await _context.ResultadosAprendizaje
            .Where(r =>
                r.IdAsignacionDocente == ra.IdAsignacionDocente &&
                r.Activo &&
                r.IdResultadoAprendizaje != idRA)
            .SumAsync(r => (decimal?)r.ValorMaximo) ?? 0;

        if (totalSinEste + dto.ValorMaximo > 100)
            return BadRequest($"La suma excede 100. Actualmente sin este RA lleva {totalSinEste}.");

        ra.Nombre = dto.Nombre;
        ra.ValorMaximo = dto.ValorMaximo;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            ra.IdResultadoAprendizaje,
            ra.IdAsignacionDocente,
            ra.Codigo,
            ra.Nombre,
            ra.ValorMaximo,
            ra.Activo
        });
    }

    [HttpDelete("{idRA}")]
    public async Task<IActionResult> EliminarRA(int idRA)
    {
        var ra = await _context.ResultadosAprendizaje
            .FirstOrDefaultAsync(r => r.IdResultadoAprendizaje == idRA);

        if (ra == null)
            return NotFound("RA no encontrado.");

        ra.Activo = false;
        await _context.SaveChangesAsync();

        return Ok("RA eliminado.");
    }
}

public class CrearRADto
{
    public string Nombre { get; set; } = "";
    public decimal ValorMaximo { get; set; }
}