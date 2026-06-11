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
public class CompetenciasGradoMateriaController : ControllerBase
{
    private readonly AppDbContext _context;

    public CompetenciasGradoMateriaController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var data = await _context.CompetenciasGradoMateria
            .Include(x => x.Grado)
                .ThenInclude(g => g.Nivel)
            .Include(x => x.Materia)
            .Include(x => x.Competencia)
            .OrderBy(x => x.Grado.Orden)
            .ThenBy(x => x.Materia.Nombre)
            .ThenBy(x => x.Competencia.Codigo)
            .Select(x => new
            {
                x.IdCompetenciaGradoMateria,
                x.IdGrado,
                Grado = x.Grado.Nombre,
                Nivel = x.Grado.Nivel.Nombre,
                x.IdMateria,
                Materia = x.Materia.Nombre,
                x.IdCompetencia,
                Competencia = x.Competencia.Codigo + " - " + x.Competencia.Nombre,
                x.Descripcion
            })
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("grado/{idGrado}")]
    public async Task<IActionResult> GetCompetenciasPorGrado(int idGrado)
    {
        var grado = await _context.Grados
            .Include(g => g.Nivel)
            .FirstOrDefaultAsync(g => g.IdGrado == idGrado);

        if (grado == null)
            return NotFound("Grado no encontrado.");

        List<int> idsCompetencias;

        // IDs oficiales MINERD
        // Primaria: 1=C1, 2=C2, 3=C3
        // Secundaria: 4=C1, 5=C2, 6=C3, 7=C4

        if (grado.Nivel.Nombre.Contains("Primaria"))
        {
            idsCompetencias = new() { 1, 2, 3 };
        }
        else
        {
            idsCompetencias = new() { 1, 2, 6, 7 };
        }

        var competencias = await _context.Competencias
            .Where(c => idsCompetencias.Contains(c.IdCompetencia))
            .OrderBy(c => c.IdCompetencia)
            .Select(c => new
            {
                c.IdCompetencia,
                c.Codigo,
                c.Nombre
            })
            .ToListAsync();

        return Ok(competencias);
    }

    [HttpGet("grado/{idGrado}/materia/{idMateria}")]
    public async Task<IActionResult> GetPorGradoMateria(int idGrado, int idMateria)
    {
        var data = await _context.CompetenciasGradoMateria
            .Include(x => x.Competencia)
            .Where(x => x.IdGrado == idGrado && x.IdMateria == idMateria)
            .OrderBy(x => x.Competencia.Codigo)
            .Select(x => new
            {
                x.IdCompetenciaGradoMateria,
                x.IdCompetencia,
                Codigo = x.Competencia.Codigo,
                Nombre = x.Competencia.Nombre,
                x.Descripcion
            })
            .ToListAsync();

        return Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> Asignar(AsignarCompetenciaGradoMateriaDto dto)
    {
        var grado = await _context.Grados
            .Include(g => g.Nivel)
            .FirstOrDefaultAsync(g => g.IdGrado == dto.IdGrado);

        if (grado == null)
            return BadRequest("El grado no existe.");

        if (!grado.Nivel.UsaCompetencias)
            return BadRequest("Este grado no pertenece a un nivel que use competencias.");

        var materiaExiste = await _context.Materias
            .AnyAsync(m => m.IdMateria == dto.IdMateria && m.Activa);

        if (!materiaExiste)
            return BadRequest("La materia no existe o está inactiva.");

        var competenciaExiste = await _context.Competencias
            .AnyAsync(c => c.IdCompetencia == dto.IdCompetencia && c.Activa);

        if (!competenciaExiste)
            return BadRequest("La competencia no existe o está inactiva.");

        var existe = await _context.CompetenciasGradoMateria
            .AnyAsync(x =>
                x.IdGrado == dto.IdGrado &&
                x.IdMateria == dto.IdMateria &&
                x.IdCompetencia == dto.IdCompetencia);

        if (existe)
            return BadRequest("Esta competencia ya está asignada a esta materia en este grado.");

        var registro = new CompetenciaGradoMateria
        {
            IdGrado = dto.IdGrado,
            IdMateria = dto.IdMateria,
            IdCompetencia = dto.IdCompetencia,
            Descripcion = dto.Descripcion
        };

        _context.CompetenciasGradoMateria.Add(registro);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Competencia asignada correctamente.",
            registro.IdCompetenciaGradoMateria
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var registro = await _context.CompetenciasGradoMateria.FindAsync(id);

        if (registro == null)
            return NotFound("Asignación no encontrada.");

        _context.CompetenciasGradoMateria.Remove(registro);
        await _context.SaveChangesAsync();

        return Ok("Competencia removida correctamente.");
    }
}