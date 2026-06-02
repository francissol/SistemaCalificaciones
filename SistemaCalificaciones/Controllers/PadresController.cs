using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.DTOs.Padres;

namespace SistemaCalificaciones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class PadresController : ControllerBase
{
    private readonly AppDbContext _context;

    public PadresController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var padres = await _context.Padres
            .Include(p => p.Usuario)
            .Include(p => p.PadreEstudiantes)
            .OrderBy(p => p.Nombres)
            .Select(p => new
            {
                p.IdPadre,
                p.Nombres,
                p.Apellidos,
                p.Telefono,
                p.Correo,
                p.Direccion,
                p.Ocupacion,
                p.Activo,
                Usuario = p.Usuario != null ? p.Usuario.NombreUsuario : null,
                CantidadHijos = p.PadreEstudiantes.Count
            })
            .ToListAsync();

        return Ok(padres);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var padre = await _context.Padres
            .Include(p => p.Usuario)
            .Include(p => p.PadreEstudiantes)
                .ThenInclude(pe => pe.Estudiante)
            .FirstOrDefaultAsync(p => p.IdPadre == id);

        if (padre == null)
            return NotFound("Padre no encontrado.");

        return Ok(new
        {
            padre.IdPadre,
            padre.Nombres,
            padre.Apellidos,
            padre.Telefono,
            padre.Correo,
            padre.Direccion,
            padre.Ocupacion,
            padre.Activo,
            Usuario = padre.Usuario != null ? padre.Usuario.NombreUsuario : null,
            Hijos = padre.PadreEstudiantes.Select(pe => new
            {
                pe.Estudiante.IdEstudiante,
                pe.Estudiante.Matricula,
                pe.Estudiante.Nombres,
                pe.Estudiante.Apellidos,
                pe.Parentesco
            })
        });
    }

    [HttpPut("{id}/datos-contacto")]
    public async Task<IActionResult> ActualizarDatosContacto(int id, ActualizarDatosPadreDto dto)
    {
        var padre = await _context.Padres.FindAsync(id);

        if (padre == null)
            return NotFound("Padre no encontrado.");

        padre.Telefono = dto.Telefono;
        padre.Correo = dto.Correo;
        padre.Direccion = dto.Direccion;
        padre.Ocupacion = dto.Ocupacion;
        padre.LugarTrabajo = dto.LugarTrabajo;
        padre.TelefonoLaboral = dto.TelefonoLaboral;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Datos del padre actualizados correctamente.",
            padre.IdPadre,
            padre.Telefono,
            padre.Correo,
            padre.Direccion
        });
    }

    [HttpGet("{id}/hijos")]
    public async Task<IActionResult> GetHijos(int id)
    {
        var existePadre = await _context.Padres.AnyAsync(p => p.IdPadre == id);

        if (!existePadre)
            return NotFound("Padre no encontrado.");

        var hijos = await _context.PadreEstudiantes
            .Include(pe => pe.Estudiante)
            .ThenInclude(e => e.Inscripciones)
            .ThenInclude(i => i.Curso)
            .Where(pe => pe.IdPadre == id)
            .Select(pe => new
            {
                pe.Estudiante.IdEstudiante,
                pe.Estudiante.Matricula,
                pe.Estudiante.Nombres,
                pe.Estudiante.Apellidos,
                pe.Parentesco,
                CursoActual = pe.Estudiante.Inscripciones
                    .OrderByDescending(i => i.IdInscripcion)
                    .Select(i => i.Curso.Nombre)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(hijos);
    }

    [HttpPut("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        var padre = await _context.Padres
            .Include(p => p.Usuario)
            .FirstOrDefaultAsync(p => p.IdPadre == id);

        if (padre == null)
            return NotFound("Padre no encontrado.");

        padre.Activo = !padre.Activo;

        if (padre.Usuario != null)
            padre.Usuario.Activo = padre.Activo;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Estado del padre actualizado.",
            padre.IdPadre,
            padre.Activo
        });
    }
}