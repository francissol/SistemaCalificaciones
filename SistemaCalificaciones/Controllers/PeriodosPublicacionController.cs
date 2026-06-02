using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.DTOs.PeriodosPublicacion;
using SistemaCalificaciones.Models;

namespace SistemaCalificaciones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,Maestro")]
public class PeriodosPublicacionController : ControllerBase
{
    private readonly AppDbContext _context;

    public PeriodosPublicacionController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var periodos = await _context.PeriodosPublicacion
            .Include(p => p.AnioEscolar)
            .OrderBy(p => p.FechaInicio)
            .Select(p => new
            {
                p.IdPeriodoPublicacion,
                p.Nombre,
                p.FechaInicio,
                p.FechaCierre,
                p.Activo,
                p.IdAnioEscolar,
                AnioEscolar = p.AnioEscolar.Nombre,
                DiasRestantes = EF.Functions.DateDiffDay(DateTime.Now, p.FechaCierre)
            })
            .ToListAsync();

        return Ok(periodos);
    }

    [HttpGet("anio/{idAnioEscolar}")]
    public async Task<IActionResult> GetPorAnio(int idAnioEscolar)
    {
        var periodos = await _context.PeriodosPublicacion
            .Where(p => p.IdAnioEscolar == idAnioEscolar)
            .OrderBy(p => p.FechaInicio)
            .Select(p => new
            {
                p.IdPeriodoPublicacion,
                p.Nombre,
                p.FechaInicio,
                p.FechaCierre,
                p.Activo,
                DiasRestantes = EF.Functions.DateDiffDay(DateTime.Now, p.FechaCierre)
            })
            .ToListAsync();

        return Ok(periodos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var periodo = await _context.PeriodosPublicacion
            .Include(p => p.AnioEscolar)
            .Where(p => p.IdPeriodoPublicacion == id)
            .Select(p => new
            {
                p.IdPeriodoPublicacion,
                p.Nombre,
                p.FechaInicio,
                p.FechaCierre,
                p.Activo,
                p.IdAnioEscolar,
                AnioEscolar = p.AnioEscolar.Nombre
            })
            .FirstOrDefaultAsync();

        if (periodo == null)
            return NotFound();

        return Ok(periodo);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(CrearPeriodoPublicacionDto dto)
    {
        var anio = await _context.AniosEscolares.FindAsync(dto.IdAnioEscolar);

        if (anio == null)
            return BadRequest("El año escolar no existe.");

        if (anio.Cerrado)
            return BadRequest("No puedes crear períodos en un año escolar cerrado.");

        if (dto.FechaCierre < dto.FechaInicio)
            return BadRequest("La fecha de cierre no puede ser menor que la fecha de inicio.");

        var existe = await _context.PeriodosPublicacion
            .AnyAsync(p => p.IdAnioEscolar == dto.IdAnioEscolar && p.Nombre == dto.Nombre);

        if (existe)
            return BadRequest("Ya existe un período con ese nombre en ese año escolar.");

        var periodo = new PeriodoPublicacion
        {
            IdAnioEscolar = dto.IdAnioEscolar,
            Nombre = dto.Nombre,
            FechaInicio = dto.FechaInicio,
            FechaCierre = dto.FechaCierre,
            Activo = true
        };

        _context.PeriodosPublicacion.Add(periodo);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Período creado correctamente.",
            periodo.IdPeriodoPublicacion,
            periodo.IdAnioEscolar,
            periodo.Nombre,
            periodo.FechaInicio,
            periodo.FechaCierre,
            periodo.Activo
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, CrearPeriodoPublicacionDto dto)
    {
        var periodo = await _context.PeriodosPublicacion.FindAsync(id);

        if (periodo == null)
            return NotFound("Período no encontrado.");

        if (dto.FechaCierre < dto.FechaInicio)
            return BadRequest("La fecha de cierre no puede ser menor que la fecha de inicio.");

        var duplicado = await _context.PeriodosPublicacion
            .AnyAsync(p =>
                p.IdAnioEscolar == dto.IdAnioEscolar &&
                p.Nombre == dto.Nombre &&
                p.IdPeriodoPublicacion != id);

        if (duplicado)
            return BadRequest("Ya existe otro período con ese nombre en ese año escolar.");

        periodo.IdAnioEscolar = dto.IdAnioEscolar;
        periodo.Nombre = dto.Nombre;
        periodo.FechaInicio = dto.FechaInicio;
        periodo.FechaCierre = dto.FechaCierre;

        await _context.SaveChangesAsync();

        return Ok(periodo);
    }

    [HttpPut("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        var periodo = await _context.PeriodosPublicacion.FindAsync(id);

        if (periodo == null)
            return NotFound("Período no encontrado.");

        periodo.Activo = !periodo.Activo;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Estado del período actualizado.",
            periodo.IdPeriodoPublicacion,
            periodo.Activo
        });
    }
}