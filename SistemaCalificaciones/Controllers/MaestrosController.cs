using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.DTOs.Maestros;
using SistemaCalificaciones.Models;
using SistemaCalificaciones.Services;

namespace SistemaCalificaciones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class MaestrosController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UsuarioGeneratorService _usuarioGenerator;

    public MaestrosController(AppDbContext context, UsuarioGeneratorService usuarioGenerator)
    {
        _context = context;
        _usuarioGenerator = usuarioGenerator;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var maestros = await _context.Maestros
            .Include(m => m.Usuario)
            .OrderBy(m => m.Nombres)
            .Select(m => new
            {
                m.IdMaestro,
                m.CodigoEmpleado,
                m.Nombres,
                m.Apellidos,
                m.Cedula,
                m.Telefono,
                m.Correo,
                m.Direccion,
                m.Especialidad,
                m.FechaIngreso,
                m.Activo,
                Usuario = m.Usuario != null ? m.Usuario.NombreUsuario : null
            })
            .ToListAsync();

        return Ok(maestros);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var maestro = await _context.Maestros
            .Include(m => m.Usuario)
            .FirstOrDefaultAsync(m => m.IdMaestro == id);

        if (maestro == null)
            return NotFound("Maestro no encontrado.");

        return Ok(maestro);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(CrearMaestroDto dto)
    {
        var rolMaestro = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == "Maestro");

        if (rolMaestro == null)
            return BadRequest("No existe el rol Maestro.");

        if (!string.IsNullOrWhiteSpace(dto.CodigoEmpleado))
        {
            var codigoExiste = await _context.Maestros
                .AnyAsync(m => m.CodigoEmpleado == dto.CodigoEmpleado);

            if (codigoExiste)
                return BadRequest("Ya existe un maestro con ese código de empleado.");
        }

        var nombreUsuario = await _usuarioGenerator.GenerarNombreUsuarioAsync(dto.Nombres, dto.Apellidos);
        var passwordTemporal = _usuarioGenerator.GenerarPasswordTemporal();

        var usuario = new Usuario
        {
            IdRol = rolMaestro.IdRol,
            NombreUsuario = nombreUsuario,
            PasswordHash = _usuarioGenerator.HashearPassword(passwordTemporal),
            DebeCambiarPassword = true,
            Activo = true,
            FechaCreacion = DateTime.Now
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        var maestro = new Maestro
        {
            IdUsuario = usuario.IdUsuario,
            CodigoEmpleado = dto.CodigoEmpleado,
            Nombres = dto.Nombres,
            Apellidos = dto.Apellidos,
            Cedula = dto.Cedula,
            Telefono = dto.Telefono,
            Correo = dto.Correo,
            Direccion = dto.Direccion,
            Especialidad = dto.Especialidad,
            FechaIngreso = dto.FechaIngreso,
            Activo = true
        };

        _context.Maestros.Add(maestro);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Maestro creado correctamente.",
            maestro.IdMaestro,
            maestro.Nombres,
            maestro.Apellidos,
            Usuario = nombreUsuario,
            PasswordTemporal = passwordTemporal
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, CrearMaestroDto dto)
    {
        var maestro = await _context.Maestros.FindAsync(id);

        if (maestro == null)
            return NotFound("Maestro no encontrado.");

        if (!string.IsNullOrWhiteSpace(dto.CodigoEmpleado))
        {
            var codigoExiste = await _context.Maestros
                .AnyAsync(m => m.CodigoEmpleado == dto.CodigoEmpleado && m.IdMaestro != id);

            if (codigoExiste)
                return BadRequest("Ya existe otro maestro con ese código de empleado.");
        }

        maestro.CodigoEmpleado = dto.CodigoEmpleado;
        maestro.Nombres = dto.Nombres;
        maestro.Apellidos = dto.Apellidos;
        maestro.Cedula = dto.Cedula;
        maestro.Telefono = dto.Telefono;
        maestro.Correo = dto.Correo;
        maestro.Direccion = dto.Direccion;
        maestro.Especialidad = dto.Especialidad;
        maestro.FechaIngreso = dto.FechaIngreso;

        await _context.SaveChangesAsync();

        return Ok(maestro);
    }

    [HttpPut("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        var maestro = await _context.Maestros
            .Include(m => m.Usuario)
            .FirstOrDefaultAsync(m => m.IdMaestro == id);

        if (maestro == null)
            return NotFound("Maestro no encontrado.");

        maestro.Activo = !maestro.Activo;

        if (maestro.Usuario != null)
        {
            maestro.Usuario.Activo = maestro.Activo;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Estado del maestro actualizado.",
            maestro.IdMaestro,
            maestro.Activo
        });
    }
}
