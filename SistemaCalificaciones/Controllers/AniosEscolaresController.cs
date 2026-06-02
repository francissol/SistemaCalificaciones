using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.DTOs.AniosEscolares;
using SistemaCalificaciones.Models;

namespace SistemaCalificaciones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class AniosEscolaresController : ControllerBase
{
    private readonly AppDbContext _context;

    public AniosEscolaresController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var anios = await _context.AniosEscolares
            .OrderByDescending(a => a.FechaInicio)
            .ToListAsync();

        return Ok(anios);
    }

    [HttpGet("activo")]
    public async Task<IActionResult> GetActivo()
    {
        var anio = await _context.AniosEscolares
            .FirstOrDefaultAsync(a => a.Activo && !a.Cerrado);

        if (anio == null)
            return NotFound("No hay año escolar activo.");

        return Ok(anio);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(CrearAnioEscolarDto dto)
    {
        var existe = await _context.AniosEscolares
            .AnyAsync(a => a.Nombre == dto.Nombre);

        if (existe)
            return BadRequest("Ya existe un año escolar con ese nombre.");

        var anio = new AnioEscolar
        {
            Nombre = dto.Nombre,
            FechaInicio = dto.FechaInicio,
            FechaFin = dto.FechaFin,
            Activo = false,
            Cerrado = false
        };

        _context.AniosEscolares.Add(anio);
        await _context.SaveChangesAsync();

        return Ok(anio);
    }

    [HttpPut("{id}/activar")]
    public async Task<IActionResult> Activar(int id)
    {
        var anio = await _context.AniosEscolares.FindAsync(id);

        if (anio == null)
            return NotFound("Año escolar no encontrado.");

        if (anio.Cerrado)
            return BadRequest("No se puede activar un año escolar cerrado.");

        var aniosActivos = await _context.AniosEscolares
            .Where(a => a.Activo)
            .ToListAsync();

        foreach (var item in aniosActivos)
        {
            item.Activo = false;
        }

        anio.Activo = true;

        await _context.SaveChangesAsync();

        return Ok("Año escolar activado correctamente.");
    }

    [HttpPut("{id}/cerrar")]
    public async Task<IActionResult> Cerrar(int id)
    {
        var anio = await _context.AniosEscolares.FindAsync(id);

        if (anio == null)
            return NotFound("Año escolar no encontrado.");

        anio.Cerrado = true;
        anio.Activo = false;

        await _context.SaveChangesAsync();

        return Ok("Año escolar cerrado correctamente.");
    }
}