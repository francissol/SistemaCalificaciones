using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;

namespace SistemaCalificaciones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
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
        var estudiantes = await _context.Estudiantes.CountAsync(e => e.Activo);
        var maestros = await _context.Maestros.CountAsync(m => m.Activo);
        var padres = await _context.Padres.CountAsync(p => p.Activo);

        var cursos = await _context.Cursos.CountAsync(c => c.Activo);

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
            anioActivo
        });
    }
}