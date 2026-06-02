using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaCalificaciones.Migrations
{
    /// <inheritdoc />
    public partial class AjusteIndiceAsignacionDocente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AsignacionesDocentes_IdCurso",
                table: "AsignacionesDocentes");

            migrationBuilder.DropIndex(
                name: "IX_AsignacionesDocentes_IdMaestro_IdCurso_IdMateria_IdAnioEscolar",
                table: "AsignacionesDocentes");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesDocentes_IdCurso_IdMateria_IdAnioEscolar",
                table: "AsignacionesDocentes",
                columns: new[] { "IdCurso", "IdMateria", "IdAnioEscolar" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesDocentes_IdMaestro",
                table: "AsignacionesDocentes",
                column: "IdMaestro");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AsignacionesDocentes_IdCurso_IdMateria_IdAnioEscolar",
                table: "AsignacionesDocentes");

            migrationBuilder.DropIndex(
                name: "IX_AsignacionesDocentes_IdMaestro",
                table: "AsignacionesDocentes");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesDocentes_IdCurso",
                table: "AsignacionesDocentes",
                column: "IdCurso");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesDocentes_IdMaestro_IdCurso_IdMateria_IdAnioEscolar",
                table: "AsignacionesDocentes",
                columns: new[] { "IdMaestro", "IdCurso", "IdMateria", "IdAnioEscolar" },
                unique: true);
        }
    }
}
