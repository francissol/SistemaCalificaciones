using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.DTOs.Materias;
using SistemaCalificaciones.Models;

namespace SistemaCalificaciones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class MateriasController : ControllerBase
{
    private readonly AppDbContext _context;

    public MateriasController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var materias = await _context.Materias
            .OrderBy(m => m.Nombre)
            .ToListAsync();

        return Ok(materias);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var materia = await _context.Materias.FindAsync(id);

        if (materia == null)
            return NotFound("Materia no encontrada.");

        return Ok(materia);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(CrearMateriaDto dto)
    {
        var existe = await _context.Materias
            .AnyAsync(m => m.Nombre == dto.Nombre);

        if (existe)
            return BadRequest("Ya existe una materia con ese nombre.");

        var materia = new Materia
        {
            Nombre = dto.Nombre,
            Abreviatura = dto.Abreviatura,
            Activa = true
        };

        _context.Materias.Add(materia);
        await _context.SaveChangesAsync();

        return Ok(materia);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, CrearMateriaDto dto)
    {
        var materia = await _context.Materias.FindAsync(id);

        if (materia == null)
            return NotFound("Materia no encontrada.");

        var duplicado = await _context.Materias
            .AnyAsync(m => m.Nombre == dto.Nombre && m.IdMateria != id);

        if (duplicado)
            return BadRequest("Ya existe otra materia con ese nombre.");

        materia.Nombre = dto.Nombre;
        materia.Abreviatura = dto.Abreviatura;

        await _context.SaveChangesAsync();

        return Ok(materia);
    }

    [HttpPut("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        var materia = await _context.Materias.FindAsync(id);

        if (materia == null)
            return NotFound("Materia no encontrada.");

        materia.Activa = !materia.Activa;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Estado de la materia actualizado.",
            materia.IdMateria,
            materia.Activa
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var materia = await _context.Materias.FindAsync(id);

        if (materia == null)
            return NotFound("Materia no encontrada.");

        var tieneAsignaciones = await _context.AsignacionesDocentes
            .AnyAsync(a => a.IdMateria == id);

        if (tieneAsignaciones)
            return BadRequest("No puedes eliminar esta materia porque tiene asignaciones docentes.");

        var tieneGrados = await _context.GradoMaterias
            .AnyAsync(gm => gm.IdMateria == id);

        if (tieneGrados)
            return BadRequest("No puedes eliminar esta materia porque está asignada a grados.");

        _context.Materias.Remove(materia);
        await _context.SaveChangesAsync();

        return Ok("Materia eliminada correctamente.");
    }
}