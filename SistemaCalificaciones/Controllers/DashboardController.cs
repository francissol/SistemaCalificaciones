using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.Helpers;

namespace SistemaCalificaciones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,CoordinadorPrimaria,CoordinadorSecundaria,CoordinadorPolitecnico")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var nivelCoordinador = NivelHelper.ObtenerNivelPorRol(User);

        var estudiantesQuery = _context.Estudiantes
            .Include(e => e.Inscripciones)
                .ThenInclude(i => i.Curso)
                    .ThenInclude(c => c.Grado)
                        .ThenInclude(g => g.Nivel)
            .Where(e => e.Activo)
            .AsQueryable();

        var cursosQuery = _context.Cursos
            .Include(c => c.Grado)
                .ThenInclude(g => g.Nivel)
            .Where(c => c.Activo)
            .AsQueryable();

        var maestrosQuery = _context.Maestros
            .Include(m => m.AsignacionesDocentes)
                .ThenInclude(a => a.Curso)
                    .ThenInclude(c => c.Grado)
                        .ThenInclude(g => g.Nivel)
            .Where(m => m.Activo)
            .AsQueryable();

        if (nivelCoordinador != null)
        {
            estudiantesQuery = estudiantesQuery.Where(e =>
                e.Inscripciones.Any(i =>
                    i.Estado == "Activo" &&
                    i.Curso.Grado.Nivel.Nombre == nivelCoordinador));

            cursosQuery = cursosQuery.Where(c =>
                c.Grado.Nivel.Nombre == nivelCoordinador);

            maestrosQuery = maestrosQuery.Where(m =>
                m.AsignacionesDocentes.Any(a =>
                    a.Curso.Grado.Nivel.Nombre == nivelCoordinador));
        }

        var estudiantes = await estudiantesQuery.CountAsync();
        var maestros = await maestrosQuery.CountAsync();
        var cursos = await cursosQuery.CountAsync();

        var padres = await _context.Padres.CountAsync(p => p.Activo);

        var materias = await _context.Materias.CountAsync(m => m.Activa);

        var publicacionesAbiertas = await _context.PeriodosPublicacion
            .CountAsync(p =>
                p.Activo &&
                p.FechaInicio <= DateTime.Now &&
                p.FechaCierre >= DateTime.Now);

        var anioActivo = await _context.AniosEscolares
            .Where(a => a.Activo)
            .Select(a => a.Nombre)
            .FirstOrDefaultAsync();

        return Ok(new
        {
            estudiantes,
            maestros,
            padres,
            cursos,
            materias,
            publicacionesAbiertas,
            anioActivo,
            nivel = nivelCoordinador ?? "Todos"
        });
    }
}