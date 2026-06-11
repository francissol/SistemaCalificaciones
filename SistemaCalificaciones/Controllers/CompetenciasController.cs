using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.DTOs.Competencias;
using SistemaCalificaciones.Models;

namespace SistemaCalificaciones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador, Maestro")]
public class CompetenciasController : ControllerBase
{
    private readonly AppDbContext _context;

    public CompetenciasController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var competencias = await _context.Competencias
            .OrderBy(c => c.Codigo)
            .ToListAsync();

        return Ok(competencias);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(CrearCompetenciaDto dto)
    {
        var existe = await _context.Competencias
            .AnyAsync(c => c.Codigo == dto.Codigo || c.Nombre == dto.Nombre);

        if (existe)
            return BadRequest("Ya existe una competencia con ese código o nombre.");

        var competencia = new Competencia
        {
            Codigo = dto.Codigo,
            Nombre = dto.Nombre,
            Activa = true
        };

        _context.Competencias.Add(competencia);
        await _context.SaveChangesAsync();

        return Ok(competencia);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, CrearCompetenciaDto dto)
    {
        var competencia = await _context.Competencias.FindAsync(id);

        if (competencia == null)
            return NotFound("Competencia no encontrada.");

        competencia.Codigo = dto.Codigo;
        competencia.Nombre = dto.Nombre;

        await _context.SaveChangesAsync();

        return Ok(competencia);
    }

    [HttpPut("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        var competencia = await _context.Competencias.FindAsync(id);

        if (competencia == null)
            return NotFound("Competencia no encontrada.");

        competencia.Activa = !competencia.Activa;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            competencia.IdCompetencia,
            competencia.Activa
        });
    }
}