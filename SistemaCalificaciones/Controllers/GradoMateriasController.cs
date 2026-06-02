using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.DTOs.GradoMaterias;
using SistemaCalificaciones.Models;

namespace SistemaCalificaciones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class GradoMateriasController : ControllerBase
{
    private readonly AppDbContext _context;

    public GradoMateriasController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var data = await _context.GradoMaterias
            .Include(gm => gm.Grado)
            .Include(gm => gm.Materia)
            .OrderBy(gm => gm.Grado.Orden)
            .ThenBy(gm => gm.Materia.Nombre)
            .Select(gm => new
            {
                gm.IdGradoMateria,
                gm.IdGrado,
                Grado = gm.Grado.Nombre,
                gm.IdMateria,
                Materia = gm.Materia.Nombre,
                Abreviatura = gm.Materia.Abreviatura
            })
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("grado/{idGrado}")]
    public async Task<IActionResult> GetPorGrado(int idGrado)
    {
        var materias = await _context.GradoMaterias
            .Include(gm => gm.Materia)
            .Where(gm => gm.IdGrado == idGrado)
            .OrderBy(gm => gm.Materia.Nombre)
            .Select(gm => new
            {
                gm.IdGradoMateria,
                gm.IdMateria,
                Materia = gm.Materia.Nombre,
                Abreviatura = gm.Materia.Abreviatura
            })
            .ToListAsync();

        return Ok(materias);
    }

    [HttpPost]
    public async Task<IActionResult> Asignar(AsignarMateriaGradoDto dto)
    {
        var gradoExiste = await _context.Grados.AnyAsync(g => g.IdGrado == dto.IdGrado);
        if (!gradoExiste)
            return BadRequest("El grado seleccionado no existe.");

        var materiaExiste = await _context.Materias.AnyAsync(m => m.IdMateria == dto.IdMateria);
        if (!materiaExiste)
            return BadRequest("La materia seleccionada no existe.");

        var existe = await _context.GradoMaterias
            .AnyAsync(gm => gm.IdGrado == dto.IdGrado && gm.IdMateria == dto.IdMateria);

        if (existe)
            return BadRequest("Esta materia ya está asignada a este grado.");

        var asignacion = new GradoMateria
        {
            IdGrado = dto.IdGrado,
            IdMateria = dto.IdMateria
        };

        _context.GradoMaterias.Add(asignacion);
        await _context.SaveChangesAsync();

        return Ok(asignacion);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var asignacion = await _context.GradoMaterias.FindAsync(id);

        if (asignacion == null)
            return NotFound("Asignación no encontrada.");

        _context.GradoMaterias.Remove(asignacion);
        await _context.SaveChangesAsync();

        return Ok("Materia removida del grado correctamente.");
    }
}