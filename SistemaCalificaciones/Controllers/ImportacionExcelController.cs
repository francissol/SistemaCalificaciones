using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.Models;
using SistemaCalificaciones.Services;

namespace SistemaCalificaciones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class ImportacionExcelController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UsuarioGeneratorService _usuarioGenerator;

    public ImportacionExcelController(AppDbContext context, UsuarioGeneratorService usuarioGenerator)
    {
        _context = context;
        _usuarioGenerator = usuarioGenerator;
    }

    [HttpPost("estudiantes")]
    public async Task<IActionResult> ImportarEstudiantes(IFormFile archivo, int idAnioEscolar)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest("Debe subir un archivo Excel.");

        var anio = await _context.AniosEscolares.FindAsync(idAnioEscolar);

        if (anio == null || anio.Cerrado)
            return BadRequest("El año escolar no existe o está cerrado.");

        var rolEstudiante = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == "Estudiante");
        var rolPadre = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == "Padre");

        if (rolEstudiante == null || rolPadre == null)
            return BadRequest("Faltan roles en la base de datos.");

        int total = 0;
        int exitosos = 0;
        int fallidos = 0;

        var errores = new List<object>();

        using var stream = new MemoryStream();
        await archivo.CopyToAsync(stream);

        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet(1);
        var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

        foreach (var row in rows)
        {
            total++;

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                string matricula = row.Cell(1).GetString().Trim();
                string nombres = row.Cell(2).GetString().Trim();
                string apellidos = row.Cell(3).GetString().Trim();

                DateTime? fechaNacimiento = null;
                if (DateTime.TryParse(row.Cell(4).GetString(), out var fechaNac))
                    fechaNacimiento = fechaNac;

                string? sexo = row.Cell(5).GetString().Trim();
                string? telefono = row.Cell(6).GetString().Trim();
                string? correo = row.Cell(7).GetString().Trim();
                string? direccion = row.Cell(8).GetString().Trim();

                string cursoNombre = row.Cell(9).GetString().Trim();
                string nivelNombre = row.Cell(10).GetString().Trim();

                string nombrePadre = row.Cell(10).GetString().Trim();
                string apellidoPadre = row.Cell(11).GetString().Trim();
                string telefonoPadre = row.Cell(12).GetString().Trim();
                string correoPadre = row.Cell(13).GetString().Trim();
                string parentesco = row.Cell(14).GetString().Trim();

                if (string.IsNullOrWhiteSpace(matricula))
                    throw new Exception("La matrícula está vacía.");

                if (string.IsNullOrWhiteSpace(nombres) || string.IsNullOrWhiteSpace(apellidos))
                    throw new Exception("El nombre o apellido del estudiante está vacío.");

                if (string.IsNullOrWhiteSpace(cursoNombre))
                    throw new Exception("El curso está vacío.");


                if (string.IsNullOrWhiteSpace(nivelNombre))
                    throw new Exception("El nivel está vacío.");

                var existeEstudiante = await _context.Estudiantes
                    .AnyAsync(e => e.Matricula == matricula);

                if (existeEstudiante)
                    throw new Exception($"Ya existe un estudiante con matrícula {matricula}.");

                var cursoNombreNormalizado = cursoNombre.Replace(" ", "").ToLower();
                var nivelNombreNormalizado = nivelNombre.Trim().ToLower();

                var curso = await _context.Cursos
                    .Include(c => c.Grado)
                        .ThenInclude(g => g.Nivel)
                    .FirstOrDefaultAsync(c =>
                        c.Nombre.Replace(" ", "").ToLower() == cursoNombreNormalizado &&
                        c.Grado.Nivel.Nombre.ToLower() == nivelNombreNormalizado &&
                        c.Activo
                    );

                if (curso == null)
                    throw new Exception($"No existe el curso {cursoNombre} en el nivel {nivelNombre}.");

                var usuarioEstudianteNombre = await _usuarioGenerator.GenerarNombreUsuarioAsync(nombres, apellidos);
                var passwordTemporal = _usuarioGenerator.GenerarPasswordTemporal();

                var usuarioEstudiante = new Usuario
                {
                    IdRol = rolEstudiante.IdRol,
                    NombreUsuario = usuarioEstudianteNombre,
                    PasswordHash = _usuarioGenerator.HashearPassword(passwordTemporal),
                    DebeCambiarPassword = true,
                    Activo = true,
                    FechaCreacion = DateTime.Now
                };

                _context.Usuarios.Add(usuarioEstudiante);
                await _context.SaveChangesAsync();

                var estudiante = new Estudiante
                {
                    IdUsuario = usuarioEstudiante.IdUsuario,
                    Matricula = matricula,
                    Nombres = nombres,
                    Apellidos = apellidos,
                    FechaNacimiento = fechaNacimiento,
                    Sexo = sexo,
                    Telefono = telefono,
                    Correo = correo,
                    Direccion = direccion,
                    FechaIngreso = DateTime.Now,
                    Activo = true
                };

                _context.Estudiantes.Add(estudiante);
                await _context.SaveChangesAsync();

                var inscripcion = new Inscripcion
                {
                    IdEstudiante = estudiante.IdEstudiante,
                    IdCurso = curso.IdCurso,
                    IdAnioEscolar = idAnioEscolar,
                    FechaInscripcion = DateTime.Now,
                    Estado = "Activo"
                };

                _context.Inscripciones.Add(inscripcion);

                if (!string.IsNullOrWhiteSpace(nombrePadre) && !string.IsNullOrWhiteSpace(apellidoPadre))
                {
                    var padre = await _context.Padres
                        .FirstOrDefaultAsync(p =>
                            p.Nombres == nombrePadre &&
                            p.Apellidos == apellidoPadre &&
                            p.Telefono == telefonoPadre);

                    if (padre == null)
                    {
                        var usuarioPadreNombre = await _usuarioGenerator.GenerarNombreUsuarioAsync(nombrePadre, apellidoPadre);

                        var usuarioPadre = new Usuario
                        {
                            IdRol = rolPadre.IdRol,
                            NombreUsuario = usuarioPadreNombre,
                            PasswordHash = _usuarioGenerator.HashearPassword(passwordTemporal),
                            DebeCambiarPassword = true,
                            Activo = true,
                            FechaCreacion = DateTime.Now
                        };

                        _context.Usuarios.Add(usuarioPadre);
                        await _context.SaveChangesAsync();

                        padre = new Padre
                        {
                            IdUsuario = usuarioPadre.IdUsuario,
                            Nombres = nombrePadre,
                            Apellidos = apellidoPadre,
                            Telefono = telefonoPadre,
                            Correo = correoPadre,
                            Activo = true
                        };

                        _context.Padres.Add(padre);
                        await _context.SaveChangesAsync();
                    }

                    _context.PadreEstudiantes.Add(new PadreEstudiante
                    {
                        IdPadre = padre.IdPadre,
                        IdEstudiante = estudiante.IdEstudiante,
                        Parentesco = string.IsNullOrWhiteSpace(parentesco) ? "Padre" : parentesco,
                        ResponsableAcademico = true
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                exitosos++;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                fallidos++;

                errores.Add(new
                {
                    Fila = row.RowNumber(),
                    Error = ex.Message
                });
            }
        }

        return Ok(new
        {
            mensaje = "Importación finalizada.",
            total,
            exitosos,
            fallidos,
            errores
        });
    }

    [HttpPost("maestros")]
    public async Task<IActionResult> ImportarMaestros(IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest("Debe subir un archivo Excel.");

        var rolMaestro = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == "Maestro");

        if (rolMaestro == null)
            return BadRequest("No existe el rol Maestro.");

        int total = 0;
        int exitosos = 0;
        int fallidos = 0;

        var errores = new List<object>();

        using var stream = new MemoryStream();
        await archivo.CopyToAsync(stream);

        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet(1);
        var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

        foreach (var row in rows)
        {
            total++;

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                string codigoEmpleado = row.Cell(1).GetString().Trim();
                string nombres = row.Cell(2).GetString().Trim();
                string apellidos = row.Cell(3).GetString().Trim();
                string cedula = row.Cell(4).GetString().Trim();
                string telefono = row.Cell(5).GetString().Trim();
                string correo = row.Cell(6).GetString().Trim();
                string direccion = row.Cell(7).GetString().Trim();
                string especialidad = row.Cell(8).GetString().Trim();

                DateTime? fechaIngreso = null;
                if (DateTime.TryParse(row.Cell(9).GetString(), out var fechaIng))
                    fechaIngreso = fechaIng;

                if (string.IsNullOrWhiteSpace(nombres) || string.IsNullOrWhiteSpace(apellidos))
                    throw new Exception("El nombre o apellido del maestro está vacío.");

                if (!string.IsNullOrWhiteSpace(codigoEmpleado))
                {
                    var existeCodigo = await _context.Maestros
                        .AnyAsync(m => m.CodigoEmpleado == codigoEmpleado);

                    if (existeCodigo)
                        throw new Exception($"Ya existe un maestro con el código {codigoEmpleado}.");
                }

                if (!string.IsNullOrWhiteSpace(cedula))
                {
                    var existeCedula = await _context.Maestros
                        .AnyAsync(m => m.Cedula == cedula);

                    if (existeCedula)
                        throw new Exception($"Ya existe un maestro con la cédula {cedula}.");
                }

                var nombreUsuario = await _usuarioGenerator.GenerarNombreUsuarioAsync(nombres, apellidos);
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
                    CodigoEmpleado = codigoEmpleado,
                    Nombres = nombres,
                    Apellidos = apellidos,
                    Cedula = cedula,
                    Telefono = telefono,
                    Correo = correo,
                    Direccion = direccion,
                    Especialidad = especialidad,
                    FechaIngreso = fechaIngreso,
                    Activo = true
                };

                _context.Maestros.Add(maestro);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                exitosos++;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                fallidos++;

                errores.Add(new
                {
                    Fila = row.RowNumber(),
                    Error = ex.Message
                });
            }
        }

        return Ok(new
        {
            mensaje = "Importación de maestros finalizada.",
            total,
            exitosos,
            fallidos,
            errores
        });
    }
}