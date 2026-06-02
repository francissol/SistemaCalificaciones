using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SistemaCalificaciones.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AniosEscolares",
                columns: table => new
                {
                    IdAnioEscolar = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    Cerrado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AniosEscolares", x => x.IdAnioEscolar);
                });

            migrationBuilder.CreateTable(
                name: "Materias",
                columns: table => new
                {
                    IdMateria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Abreviatura = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materias", x => x.IdMateria);
                });

            migrationBuilder.CreateTable(
                name: "Niveles",
                columns: table => new
                {
                    IdNivel = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Niveles", x => x.IdNivel);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    IdRol = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.IdRol);
                });

            migrationBuilder.CreateTable(
                name: "PeriodosPublicacion",
                columns: table => new
                {
                    IdPeriodoPublicacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdAnioEscolar = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodosPublicacion", x => x.IdPeriodoPublicacion);
                    table.ForeignKey(
                        name: "FK_PeriodosPublicacion_AniosEscolares_IdAnioEscolar",
                        column: x => x.IdAnioEscolar,
                        principalTable: "AniosEscolares",
                        principalColumn: "IdAnioEscolar",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Grados",
                columns: table => new
                {
                    IdGrado = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdNivel = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grados", x => x.IdGrado);
                    table.ForeignKey(
                        name: "FK_Grados_Niveles_IdNivel",
                        column: x => x.IdNivel,
                        principalTable: "Niveles",
                        principalColumn: "IdNivel",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdRol = table.Column<int>(type: "int", nullable: false),
                    NombreUsuario = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DebeCambiarPassword = table.Column<bool>(type: "bit", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    UltimoAcceso = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.IdUsuario);
                    table.ForeignKey(
                        name: "FK_Usuarios_Roles_IdRol",
                        column: x => x.IdRol,
                        principalTable: "Roles",
                        principalColumn: "IdRol",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cursos",
                columns: table => new
                {
                    IdCurso = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdGrado = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Seccion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cursos", x => x.IdCurso);
                    table.ForeignKey(
                        name: "FK_Cursos_Grados_IdGrado",
                        column: x => x.IdGrado,
                        principalTable: "Grados",
                        principalColumn: "IdGrado",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GradoMaterias",
                columns: table => new
                {
                    IdGradoMateria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdGrado = table.Column<int>(type: "int", nullable: false),
                    IdMateria = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradoMaterias", x => x.IdGradoMateria);
                    table.ForeignKey(
                        name: "FK_GradoMaterias_Grados_IdGrado",
                        column: x => x.IdGrado,
                        principalTable: "Grados",
                        principalColumn: "IdGrado",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GradoMaterias_Materias_IdMateria",
                        column: x => x.IdMateria,
                        principalTable: "Materias",
                        principalColumn: "IdMateria",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Estudiantes",
                columns: table => new
                {
                    IdEstudiante = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<int>(type: "int", nullable: true),
                    Matricula = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Nombres = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Sexo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Correo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaIngreso = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estudiantes", x => x.IdEstudiante);
                    table.ForeignKey(
                        name: "FK_Estudiantes_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Maestros",
                columns: table => new
                {
                    IdMaestro = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<int>(type: "int", nullable: true),
                    CodigoEmpleado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nombres = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cedula = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Correo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Especialidad = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaIngreso = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maestros", x => x.IdMaestro);
                    table.ForeignKey(
                        name: "FK_Maestros_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Padres",
                columns: table => new
                {
                    IdPadre = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<int>(type: "int", nullable: true),
                    Nombres = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cedula = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Correo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ocupacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LugarTrabajo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TelefonoLaboral = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Padres", x => x.IdPadre);
                    table.ForeignKey(
                        name: "FK_Padres_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Inscripciones",
                columns: table => new
                {
                    IdInscripcion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdEstudiante = table.Column<int>(type: "int", nullable: false),
                    IdCurso = table.Column<int>(type: "int", nullable: false),
                    IdAnioEscolar = table.Column<int>(type: "int", nullable: false),
                    FechaInscripcion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inscripciones", x => x.IdInscripcion);
                    table.ForeignKey(
                        name: "FK_Inscripciones_AniosEscolares_IdAnioEscolar",
                        column: x => x.IdAnioEscolar,
                        principalTable: "AniosEscolares",
                        principalColumn: "IdAnioEscolar",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inscripciones_Cursos_IdCurso",
                        column: x => x.IdCurso,
                        principalTable: "Cursos",
                        principalColumn: "IdCurso",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inscripciones_Estudiantes_IdEstudiante",
                        column: x => x.IdEstudiante,
                        principalTable: "Estudiantes",
                        principalColumn: "IdEstudiante",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AsignacionesDocentes",
                columns: table => new
                {
                    IdAsignacionDocente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdMaestro = table.Column<int>(type: "int", nullable: false),
                    IdCurso = table.Column<int>(type: "int", nullable: false),
                    IdMateria = table.Column<int>(type: "int", nullable: false),
                    IdAnioEscolar = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsignacionesDocentes", x => x.IdAsignacionDocente);
                    table.ForeignKey(
                        name: "FK_AsignacionesDocentes_AniosEscolares_IdAnioEscolar",
                        column: x => x.IdAnioEscolar,
                        principalTable: "AniosEscolares",
                        principalColumn: "IdAnioEscolar",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AsignacionesDocentes_Cursos_IdCurso",
                        column: x => x.IdCurso,
                        principalTable: "Cursos",
                        principalColumn: "IdCurso",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AsignacionesDocentes_Maestros_IdMaestro",
                        column: x => x.IdMaestro,
                        principalTable: "Maestros",
                        principalColumn: "IdMaestro",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AsignacionesDocentes_Materias_IdMateria",
                        column: x => x.IdMateria,
                        principalTable: "Materias",
                        principalColumn: "IdMateria",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PadreEstudiantes",
                columns: table => new
                {
                    IdPadreEstudiante = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPadre = table.Column<int>(type: "int", nullable: false),
                    IdEstudiante = table.Column<int>(type: "int", nullable: false),
                    Parentesco = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponsableAcademico = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PadreEstudiantes", x => x.IdPadreEstudiante);
                    table.ForeignKey(
                        name: "FK_PadreEstudiantes_Estudiantes_IdEstudiante",
                        column: x => x.IdEstudiante,
                        principalTable: "Estudiantes",
                        principalColumn: "IdEstudiante",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PadreEstudiantes_Padres_IdPadre",
                        column: x => x.IdPadre,
                        principalTable: "Padres",
                        principalColumn: "IdPadre",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActividadesEvaluativas",
                columns: table => new
                {
                    IdActividadEvaluativa = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdAsignacionDocente = table.Column<int>(type: "int", nullable: false),
                    IdPeriodoPublicacion = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Porcentaje = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActividadesEvaluativas", x => x.IdActividadEvaluativa);
                    table.ForeignKey(
                        name: "FK_ActividadesEvaluativas_AsignacionesDocentes_IdAsignacionDocente",
                        column: x => x.IdAsignacionDocente,
                        principalTable: "AsignacionesDocentes",
                        principalColumn: "IdAsignacionDocente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActividadesEvaluativas_PeriodosPublicacion_IdPeriodoPublicacion",
                        column: x => x.IdPeriodoPublicacion,
                        principalTable: "PeriodosPublicacion",
                        principalColumn: "IdPeriodoPublicacion",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CalificacionesPeriodo",
                columns: table => new
                {
                    IdCalificacionPeriodo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdEstudiante = table.Column<int>(type: "int", nullable: false),
                    IdAsignacionDocente = table.Column<int>(type: "int", nullable: false),
                    IdPeriodoPublicacion = table.Column<int>(type: "int", nullable: false),
                    NotaFinal = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Publicada = table.Column<bool>(type: "bit", nullable: false),
                    FechaPublicacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalificacionesPeriodo", x => x.IdCalificacionPeriodo);
                    table.ForeignKey(
                        name: "FK_CalificacionesPeriodo_AsignacionesDocentes_IdAsignacionDocente",
                        column: x => x.IdAsignacionDocente,
                        principalTable: "AsignacionesDocentes",
                        principalColumn: "IdAsignacionDocente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalificacionesPeriodo_Estudiantes_IdEstudiante",
                        column: x => x.IdEstudiante,
                        principalTable: "Estudiantes",
                        principalColumn: "IdEstudiante",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalificacionesPeriodo_PeriodosPublicacion_IdPeriodoPublicacion",
                        column: x => x.IdPeriodoPublicacion,
                        principalTable: "PeriodosPublicacion",
                        principalColumn: "IdPeriodoPublicacion",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Observaciones",
                columns: table => new
                {
                    IdObservacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdEstudiante = table.Column<int>(type: "int", nullable: false),
                    IdAsignacionDocente = table.Column<int>(type: "int", nullable: false),
                    IdPeriodoPublicacion = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Comentario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Observaciones", x => x.IdObservacion);
                    table.ForeignKey(
                        name: "FK_Observaciones_AsignacionesDocentes_IdAsignacionDocente",
                        column: x => x.IdAsignacionDocente,
                        principalTable: "AsignacionesDocentes",
                        principalColumn: "IdAsignacionDocente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Observaciones_Estudiantes_IdEstudiante",
                        column: x => x.IdEstudiante,
                        principalTable: "Estudiantes",
                        principalColumn: "IdEstudiante",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Observaciones_PeriodosPublicacion_IdPeriodoPublicacion",
                        column: x => x.IdPeriodoPublicacion,
                        principalTable: "PeriodosPublicacion",
                        principalColumn: "IdPeriodoPublicacion",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotasActividades",
                columns: table => new
                {
                    IdNotaActividad = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdActividadEvaluativa = table.Column<int>(type: "int", nullable: false),
                    IdEstudiante = table.Column<int>(type: "int", nullable: false),
                    Nota = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaUltimaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotasActividades", x => x.IdNotaActividad);
                    table.ForeignKey(
                        name: "FK_NotasActividades_ActividadesEvaluativas_IdActividadEvaluativa",
                        column: x => x.IdActividadEvaluativa,
                        principalTable: "ActividadesEvaluativas",
                        principalColumn: "IdActividadEvaluativa",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotasActividades_Estudiantes_IdEstudiante",
                        column: x => x.IdEstudiante,
                        principalTable: "Estudiantes",
                        principalColumn: "IdEstudiante",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "IdRol", "Nombre" },
                values: new object[,]
                {
                    { 1, "Administrador" },
                    { 2, "Maestro" },
                    { 3, "Estudiante" },
                    { 4, "Padre" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActividadesEvaluativas_IdAsignacionDocente",
                table: "ActividadesEvaluativas",
                column: "IdAsignacionDocente");

            migrationBuilder.CreateIndex(
                name: "IX_ActividadesEvaluativas_IdPeriodoPublicacion",
                table: "ActividadesEvaluativas",
                column: "IdPeriodoPublicacion");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesDocentes_IdAnioEscolar",
                table: "AsignacionesDocentes",
                column: "IdAnioEscolar");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesDocentes_IdCurso",
                table: "AsignacionesDocentes",
                column: "IdCurso");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesDocentes_IdMaestro_IdCurso_IdMateria_IdAnioEscolar",
                table: "AsignacionesDocentes",
                columns: new[] { "IdMaestro", "IdCurso", "IdMateria", "IdAnioEscolar" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesDocentes_IdMateria",
                table: "AsignacionesDocentes",
                column: "IdMateria");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesPeriodo_IdAsignacionDocente",
                table: "CalificacionesPeriodo",
                column: "IdAsignacionDocente");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesPeriodo_IdEstudiante_IdAsignacionDocente_IdPeriodoPublicacion",
                table: "CalificacionesPeriodo",
                columns: new[] { "IdEstudiante", "IdAsignacionDocente", "IdPeriodoPublicacion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesPeriodo_IdPeriodoPublicacion",
                table: "CalificacionesPeriodo",
                column: "IdPeriodoPublicacion");

            migrationBuilder.CreateIndex(
                name: "IX_Cursos_IdGrado",
                table: "Cursos",
                column: "IdGrado");

            migrationBuilder.CreateIndex(
                name: "IX_Estudiantes_IdUsuario",
                table: "Estudiantes",
                column: "IdUsuario",
                unique: true,
                filter: "[IdUsuario] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Estudiantes_Matricula",
                table: "Estudiantes",
                column: "Matricula",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GradoMaterias_IdGrado_IdMateria",
                table: "GradoMaterias",
                columns: new[] { "IdGrado", "IdMateria" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GradoMaterias_IdMateria",
                table: "GradoMaterias",
                column: "IdMateria");

            migrationBuilder.CreateIndex(
                name: "IX_Grados_IdNivel",
                table: "Grados",
                column: "IdNivel");

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_IdAnioEscolar",
                table: "Inscripciones",
                column: "IdAnioEscolar");

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_IdCurso",
                table: "Inscripciones",
                column: "IdCurso");

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_IdEstudiante_IdAnioEscolar",
                table: "Inscripciones",
                columns: new[] { "IdEstudiante", "IdAnioEscolar" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Maestros_IdUsuario",
                table: "Maestros",
                column: "IdUsuario",
                unique: true,
                filter: "[IdUsuario] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_NotasActividades_IdActividadEvaluativa_IdEstudiante",
                table: "NotasActividades",
                columns: new[] { "IdActividadEvaluativa", "IdEstudiante" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotasActividades_IdEstudiante",
                table: "NotasActividades",
                column: "IdEstudiante");

            migrationBuilder.CreateIndex(
                name: "IX_Observaciones_IdAsignacionDocente",
                table: "Observaciones",
                column: "IdAsignacionDocente");

            migrationBuilder.CreateIndex(
                name: "IX_Observaciones_IdEstudiante",
                table: "Observaciones",
                column: "IdEstudiante");

            migrationBuilder.CreateIndex(
                name: "IX_Observaciones_IdPeriodoPublicacion",
                table: "Observaciones",
                column: "IdPeriodoPublicacion");

            migrationBuilder.CreateIndex(
                name: "IX_PadreEstudiantes_IdEstudiante",
                table: "PadreEstudiantes",
                column: "IdEstudiante");

            migrationBuilder.CreateIndex(
                name: "IX_PadreEstudiantes_IdPadre_IdEstudiante",
                table: "PadreEstudiantes",
                columns: new[] { "IdPadre", "IdEstudiante" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Padres_IdUsuario",
                table: "Padres",
                column: "IdUsuario",
                unique: true,
                filter: "[IdUsuario] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodosPublicacion_IdAnioEscolar",
                table: "PeriodosPublicacion",
                column: "IdAnioEscolar");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_IdRol",
                table: "Usuarios",
                column: "IdRol");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_NombreUsuario",
                table: "Usuarios",
                column: "NombreUsuario",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalificacionesPeriodo");

            migrationBuilder.DropTable(
                name: "GradoMaterias");

            migrationBuilder.DropTable(
                name: "Inscripciones");

            migrationBuilder.DropTable(
                name: "NotasActividades");

            migrationBuilder.DropTable(
                name: "Observaciones");

            migrationBuilder.DropTable(
                name: "PadreEstudiantes");

            migrationBuilder.DropTable(
                name: "ActividadesEvaluativas");

            migrationBuilder.DropTable(
                name: "Estudiantes");

            migrationBuilder.DropTable(
                name: "Padres");

            migrationBuilder.DropTable(
                name: "AsignacionesDocentes");

            migrationBuilder.DropTable(
                name: "PeriodosPublicacion");

            migrationBuilder.DropTable(
                name: "Cursos");

            migrationBuilder.DropTable(
                name: "Maestros");

            migrationBuilder.DropTable(
                name: "Materias");

            migrationBuilder.DropTable(
                name: "AniosEscolares");

            migrationBuilder.DropTable(
                name: "Grados");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Niveles");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
