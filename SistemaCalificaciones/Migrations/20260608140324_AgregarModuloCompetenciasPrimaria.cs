using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaCalificaciones.Migrations
{
    /// <inheritdoc />
    public partial class AgregarModuloCompetenciasPrimaria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "UsaCompetencias",
                table: "Niveles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ActividadesCompetencias",
                columns: table => new
                {
                    IdActividadCompetencia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdAsignacionDocente = table.Column<int>(type: "int", nullable: false),
                    IdPeriodoPublicacion = table.Column<int>(type: "int", nullable: false),
                    IdCompetencia = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActividadesCompetencias", x => x.IdActividadCompetencia);
                });

            migrationBuilder.CreateTable(
                name: "Competencias",
                columns: table => new
                {
                    IdCompetencia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Competencias", x => x.IdCompetencia);
                });

            migrationBuilder.CreateTable(
                name: "NotasCompetencias",
                columns: table => new
                {
                    IdNotaCompetencia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdActividadCompetencia = table.Column<int>(type: "int", nullable: false),
                    IdEstudiante = table.Column<int>(type: "int", nullable: false),
                    Nota = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotasCompetencias", x => x.IdNotaCompetencia);
                });

            migrationBuilder.CreateTable(
                name: "CalificacionesCompetenciasPeriodo",
                columns: table => new
                {
                    IdCalificacionCompetenciaPeriodo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdEstudiante = table.Column<int>(type: "int", nullable: false),
                    EstudianteIdEstudiante = table.Column<int>(type: "int", nullable: false),
                    IdAsignacionDocente = table.Column<int>(type: "int", nullable: false),
                    AsignacionDocenteIdAsignacionDocente = table.Column<int>(type: "int", nullable: false),
                    IdPeriodoPublicacion = table.Column<int>(type: "int", nullable: false),
                    PeriodoPublicacionIdPeriodoPublicacion = table.Column<int>(type: "int", nullable: false),
                    IdCompetencia = table.Column<int>(type: "int", nullable: false),
                    CompetenciaIdCompetencia = table.Column<int>(type: "int", nullable: false),
                    Promedio = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Publicada = table.Column<bool>(type: "bit", nullable: false),
                    FechaPublicacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalificacionesCompetenciasPeriodo", x => x.IdCalificacionCompetenciaPeriodo);
                    table.ForeignKey(
                        name: "FK_CalificacionesCompetenciasPeriodo_AsignacionesDocentes_AsignacionDocenteIdAsignacionDocente",
                        column: x => x.AsignacionDocenteIdAsignacionDocente,
                        principalTable: "AsignacionesDocentes",
                        principalColumn: "IdAsignacionDocente",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CalificacionesCompetenciasPeriodo_Competencias_CompetenciaIdCompetencia",
                        column: x => x.CompetenciaIdCompetencia,
                        principalTable: "Competencias",
                        principalColumn: "IdCompetencia",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CalificacionesCompetenciasPeriodo_Estudiantes_EstudianteIdEstudiante",
                        column: x => x.EstudianteIdEstudiante,
                        principalTable: "Estudiantes",
                        principalColumn: "IdEstudiante",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CalificacionesCompetenciasPeriodo_PeriodosPublicacion_PeriodoPublicacionIdPeriodoPublicacion",
                        column: x => x.PeriodoPublicacionIdPeriodoPublicacion,
                        principalTable: "PeriodosPublicacion",
                        principalColumn: "IdPeriodoPublicacion",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompetenciasGradoMateria",
                columns: table => new
                {
                    IdCompetenciaGradoMateria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdGrado = table.Column<int>(type: "int", nullable: false),
                    IdMateria = table.Column<int>(type: "int", nullable: false),
                    IdCompetencia = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GradoIdGrado = table.Column<int>(type: "int", nullable: false),
                    MateriaIdMateria = table.Column<int>(type: "int", nullable: false),
                    CompetenciaIdCompetencia = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetenciasGradoMateria", x => x.IdCompetenciaGradoMateria);
                    table.ForeignKey(
                        name: "FK_CompetenciasGradoMateria_Competencias_CompetenciaIdCompetencia",
                        column: x => x.CompetenciaIdCompetencia,
                        principalTable: "Competencias",
                        principalColumn: "IdCompetencia",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompetenciasGradoMateria_Grados_GradoIdGrado",
                        column: x => x.GradoIdGrado,
                        principalTable: "Grados",
                        principalColumn: "IdGrado",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompetenciasGradoMateria_Materias_MateriaIdMateria",
                        column: x => x.MateriaIdMateria,
                        principalTable: "Materias",
                        principalColumn: "IdMateria",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesCompetenciasPeriodo_AsignacionDocenteIdAsignacionDocente",
                table: "CalificacionesCompetenciasPeriodo",
                column: "AsignacionDocenteIdAsignacionDocente");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesCompetenciasPeriodo_CompetenciaIdCompetencia",
                table: "CalificacionesCompetenciasPeriodo",
                column: "CompetenciaIdCompetencia");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesCompetenciasPeriodo_EstudianteIdEstudiante",
                table: "CalificacionesCompetenciasPeriodo",
                column: "EstudianteIdEstudiante");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesCompetenciasPeriodo_PeriodoPublicacionIdPeriodoPublicacion",
                table: "CalificacionesCompetenciasPeriodo",
                column: "PeriodoPublicacionIdPeriodoPublicacion");

            migrationBuilder.CreateIndex(
                name: "IX_CompetenciasGradoMateria_CompetenciaIdCompetencia",
                table: "CompetenciasGradoMateria",
                column: "CompetenciaIdCompetencia");

            migrationBuilder.CreateIndex(
                name: "IX_CompetenciasGradoMateria_GradoIdGrado",
                table: "CompetenciasGradoMateria",
                column: "GradoIdGrado");

            migrationBuilder.CreateIndex(
                name: "IX_CompetenciasGradoMateria_MateriaIdMateria",
                table: "CompetenciasGradoMateria",
                column: "MateriaIdMateria");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActividadesCompetencias");

            migrationBuilder.DropTable(
                name: "CalificacionesCompetenciasPeriodo");

            migrationBuilder.DropTable(
                name: "CompetenciasGradoMateria");

            migrationBuilder.DropTable(
                name: "NotasCompetencias");

            migrationBuilder.DropTable(
                name: "Competencias");

            migrationBuilder.DropColumn(
                name: "UsaCompetencias",
                table: "Niveles");
        }
    }
}
