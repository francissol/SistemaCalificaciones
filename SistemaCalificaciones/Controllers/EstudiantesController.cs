using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.DTOs.Estudiantes;
using SistemaCalificaciones.Models;
using SistemaCalificaciones.Services;
using System.Security.Claims;

namespace SistemaCalificaciones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class EstudiantesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UsuarioGeneratorService _usuarioGenerator;

    public EstudiantesController(AppDbContext context, UsuarioGeneratorService usuarioGenerator)
    {
        _context = context;
        _usuarioGenerator = usuarioGenerator;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var estudiantes = await _context.Estudiantes
            .Include(e => e.Usuario)
            .Include(e => e.Inscripciones)
                .ThenInclude(i => i.Curso)
            .OrderBy(e => e.Nombres)
            .Select(e => new
            {
                e.IdEstudiante,
                e.Matricula,
                e.Nombres,
                e.Apellidos,
                e.Telefono,
                e.Correo,
                e.Activo,
                Usuario = e.Usuario != null ? e.Usuario.NombreUsuario : null,
                CursoActual = e.Inscripciones
                    .OrderByDescending(i => i.IdInscripcion)
                    .Select(i => i.Curso.Nombre)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(estudiantes);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var estudiante = await _context.Estudiantes
            .Include(e => e.Usuario)
            .Include(e => e.PadreEstudiantes)
                .ThenInclude(pe => pe.Padre)
            .Include(e => e.Inscripciones)
                .ThenInclude(i => i.Curso)
            .FirstOrDefaultAsync(e => e.IdEstudiante == id);

        if (estudiante == null)
            return NotFound("Estudiante no encontrado.");

        return Ok(estudiante);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(CrearEstudianteDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        var rolEstudiante = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == "Estudiante");
        var rolPadre = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == "Padre");

        if (rolEstudiante == null || rolPadre == null)
            return BadRequest("Faltan roles en la base de datos.");

        var existeMatricula = await _context.Estudiantes.AnyAsync(e => e.Matricula == dto.Matricula);
        if (existeMatricula)
            return BadRequest("Ya existe un estudiante con esa matrícula.");

        var cursoExiste = await _context.Cursos.AnyAsync(c => c.IdCurso == dto.IdCurso && c.Activo);
        if (!cursoExiste)
            return BadRequest("El curso no existe o está inactivo.");

        var anioExiste = await _context.AniosEscolares.AnyAsync(a => a.IdAnioEscolar == dto.IdAnioEscolar && !a.Cerrado);
        if (!anioExiste)
            return BadRequest("El año escolar no existe o está cerrado.");

        var usuarioEstudianteNombre = await _usuarioGenerator.GenerarNombreUsuarioAsync(dto.Nombres, dto.Apellidos);
        var passwordTemporal = _usuarioGenerator.GenerarPasswordTemporal();

        var usuarioEstudiante = new Usuario
        {
            IdRol = rolEstudiante.IdRol,
            NombreUsuario = usuarioEstudianteNombre,
            PasswordHash = _usuarioGenerator.HashearPassword(passwordTemporal),
            DebeCambiarPassword = true,
            Activo = true
        };

        _context.Usuarios.Add(usuarioEstudiante);
        await _context.SaveChangesAsync();

        var estudiante = new Estudiante
        {
            IdUsuario = usuarioEstudiante.IdUsuario,
            Matricula = dto.Matricula,
            Nombres = dto.Nombres,
            Apellidos = dto.Apellidos,
            FechaNacimiento = dto.FechaNacimiento,
            Sexo = dto.Sexo,
            Telefono = dto.Telefono,
            Correo = dto.Correo,
            Direccion = dto.Direccion,
            FechaIngreso = dto.FechaIngreso,
            Activo = true
        };

        _context.Estudiantes.Add(estudiante);
        await _context.SaveChangesAsync();

        var inscripcion = new Inscripcion
        {
            IdEstudiante = estudiante.IdEstudiante,
            IdCurso = dto.IdCurso,
            IdAnioEscolar = dto.IdAnioEscolar,
            Estado = "Activo"
        };

        _context.Inscripciones.Add(inscripcion);

        Padre? padre = null;

        if (!string.IsNullOrWhiteSpace(dto.NombrePadre) && !string.IsNullOrWhiteSpace(dto.ApellidoPadre))
        {
            padre = await _context.Padres
                .FirstOrDefaultAsync(p =>
                    p.Nombres == dto.NombrePadre &&
                    p.Apellidos == dto.ApellidoPadre &&
                    p.Telefono == dto.TelefonoPadre);

            if (padre == null)
            {
                var usuarioPadreNombre = await _usuarioGenerator.GenerarNombreUsuarioAsync(dto.NombrePadre, dto.ApellidoPadre);

                var usuarioPadre = new Usuario
                {
                    IdRol = rolPadre.IdRol,
                    NombreUsuario = usuarioPadreNombre,
                    PasswordHash = _usuarioGenerator.HashearPassword(passwordTemporal),
                    DebeCambiarPassword = true,
                    Activo = true
                };

                _context.Usuarios.Add(usuarioPadre);
                await _context.SaveChangesAsync();

                padre = new Padre
                {
                    IdUsuario = usuarioPadre.IdUsuario,
                    Nombres = dto.NombrePadre,
                    Apellidos = dto.ApellidoPadre,
                    Telefono = dto.TelefonoPadre,
                    Correo = dto.CorreoPadre,
                    Activo = true
                };

                _context.Padres.Add(padre);
                await _context.SaveChangesAsync();
            }

            var relacionExiste = await _context.PadreEstudiantes
                .AnyAsync(pe => pe.IdPadre == padre.IdPadre && pe.IdEstudiante == estudiante.IdEstudiante);

            if (!relacionExiste)
            {
                _context.PadreEstudiantes.Add(new PadreEstudiante
                {
                    IdPadre = padre.IdPadre,
                    IdEstudiante = estudiante.IdEstudiante,
                    Parentesco = dto.Parentesco,
                    ResponsableAcademico = true
                });
            }
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(new
        {
            mensaje = "Estudiante creado correctamente.",
            estudiante.IdEstudiante,
            UsuarioEstudiante = usuarioEstudianteNombre,
            PasswordTemporal = passwordTemporal,
            PadreCreadoORelacionado = padre != null
        });
    }

    [HttpPut("{id}/datos-contacto")]
    public async Task<IActionResult> ActualizarDatosContacto(int id, ActualizarDatosEstudianteDto dto)
    {
        var estudiante = await _context.Estudiantes.FindAsync(id);

        if (estudiante == null)
            return NotFound("Estudiante no encontrado.");

        estudiante.Telefono = dto.Telefono;
        estudiante.Correo = dto.Correo;
        estudiante.Direccion = dto.Direccion;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Datos de contacto actualizados correctamente.",
            estudiante.IdEstudiante,
            estudiante.Telefono,
            estudiante.Correo,
            estudiante.Direccion
        });
    }

   

    [HttpPut("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        var estudiante = await _context.Estudiantes
            .Include(e => e.Usuario)
            .FirstOrDefaultAsync(e => e.IdEstudiante == id);

        if (estudiante == null)
            return NotFound("Estudiante no encontrado.");

        estudiante.Activo = !estudiante.Activo;

        if (estudiante.Usuario != null)
            estudiante.Usuario.Activo = estudiante.Activo;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Estado del estudiante actualizado.",
            estudiante.IdEstudiante,
            estudiante.Activo
        });
    }
}