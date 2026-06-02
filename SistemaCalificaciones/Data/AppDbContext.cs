using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Models;

namespace SistemaCalificaciones.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Estudiante> Estudiantes => Set<Estudiante>();
    public DbSet<Maestro> Maestros => Set<Maestro>();
    public DbSet<Padre> Padres => Set<Padre>();
    public DbSet<PadreEstudiante> PadreEstudiantes => Set<PadreEstudiante>();

    public DbSet<AnioEscolar> AniosEscolares => Set<AnioEscolar>();
    public DbSet<Nivel> Niveles => Set<Nivel>();
    public DbSet<Grado> Grados => Set<Grado>();
    public DbSet<Curso> Cursos => Set<Curso>();
    public DbSet<Materia> Materias => Set<Materia>();
    public DbSet<GradoMateria> GradoMaterias => Set<GradoMateria>();
    public DbSet<Inscripcion> Inscripciones => Set<Inscripcion>();

    public DbSet<AsignacionDocente> AsignacionesDocentes => Set<AsignacionDocente>();
    public DbSet<PeriodoPublicacion> PeriodosPublicacion => Set<PeriodoPublicacion>();
    public DbSet<ActividadEvaluativa> ActividadesEvaluativas => Set<ActividadEvaluativa>();
    public DbSet<NotaActividad> NotasActividades => Set<NotaActividad>();
    public DbSet<CalificacionPeriodo> CalificacionesPeriodo => Set<CalificacionPeriodo>();
    public DbSet<Observacion> Observaciones => Set<Observacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =========================
        // LLAVES PRIMARIAS
        // =========================

        modelBuilder.Entity<Rol>().HasKey(x => x.IdRol);
        modelBuilder.Entity<Usuario>().HasKey(x => x.IdUsuario);
        modelBuilder.Entity<Estudiante>().HasKey(x => x.IdEstudiante);
        modelBuilder.Entity<Maestro>().HasKey(x => x.IdMaestro);
        modelBuilder.Entity<Padre>().HasKey(x => x.IdPadre);
        modelBuilder.Entity<PadreEstudiante>().HasKey(x => x.IdPadreEstudiante);

        modelBuilder.Entity<AnioEscolar>().HasKey(x => x.IdAnioEscolar);
        modelBuilder.Entity<Nivel>().HasKey(x => x.IdNivel);
        modelBuilder.Entity<Grado>().HasKey(x => x.IdGrado);
        modelBuilder.Entity<Curso>().HasKey(x => x.IdCurso);
        modelBuilder.Entity<Materia>().HasKey(x => x.IdMateria);
        modelBuilder.Entity<GradoMateria>().HasKey(x => x.IdGradoMateria);
        modelBuilder.Entity<Inscripcion>().HasKey(x => x.IdInscripcion);

        modelBuilder.Entity<AsignacionDocente>().HasKey(x => x.IdAsignacionDocente);
        modelBuilder.Entity<PeriodoPublicacion>().HasKey(x => x.IdPeriodoPublicacion);
        modelBuilder.Entity<ActividadEvaluativa>().HasKey(x => x.IdActividadEvaluativa);
        modelBuilder.Entity<NotaActividad>().HasKey(x => x.IdNotaActividad);
        modelBuilder.Entity<CalificacionPeriodo>().HasKey(x => x.IdCalificacionPeriodo);
        modelBuilder.Entity<Observacion>().HasKey(x => x.IdObservacion);

        // =========================
        // DATOS INICIALES
        // =========================

        modelBuilder.Entity<Rol>().HasData(
            new Rol { IdRol = 1, Nombre = "Administrador" },
            new Rol { IdRol = 2, Nombre = "Maestro" },
            new Rol { IdRol = 3, Nombre = "Estudiante" },
            new Rol { IdRol = 4, Nombre = "Padre" }
        );

        // =========================
        // RELACIONES
        // =========================

        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Rol)
            .WithMany(r => r.Usuarios)
            .HasForeignKey(u => u.IdRol)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Estudiante>()
            .HasOne(e => e.Usuario)
            .WithOne(u => u.Estudiante)
            .HasForeignKey<Estudiante>(e => e.IdUsuario)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Maestro>()
            .HasOne(m => m.Usuario)
            .WithOne(u => u.Maestro)
            .HasForeignKey<Maestro>(m => m.IdUsuario)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Padre>()
            .HasOne(p => p.Usuario)
            .WithOne(u => u.Padre)
            .HasForeignKey<Padre>(p => p.IdUsuario)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PeriodoPublicacion>()
            .HasOne(p => p.AnioEscolar)
            .WithMany(a => a.PeriodosPublicacion)
            .HasForeignKey(p => p.IdAnioEscolar)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Grado>()
            .HasOne(g => g.Nivel)
            .WithMany(n => n.Grados)
            .HasForeignKey(g => g.IdNivel)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Curso>()
            .HasOne(c => c.Grado)
            .WithMany(g => g.Cursos)
            .HasForeignKey(c => c.IdGrado)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GradoMateria>()
            .HasOne(gm => gm.Grado)
            .WithMany(g => g.GradoMaterias)
            .HasForeignKey(gm => gm.IdGrado)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GradoMateria>()
            .HasOne(gm => gm.Materia)
            .WithMany(m => m.GradoMaterias)
            .HasForeignKey(gm => gm.IdMateria)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Inscripcion>()
            .HasOne(i => i.Estudiante)
            .WithMany(e => e.Inscripciones)
            .HasForeignKey(i => i.IdEstudiante)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Inscripcion>()
            .HasOne(i => i.Curso)
            .WithMany(c => c.Inscripciones)
            .HasForeignKey(i => i.IdCurso)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Inscripcion>()
            .HasOne(i => i.AnioEscolar)
            .WithMany(a => a.Inscripciones)
            .HasForeignKey(i => i.IdAnioEscolar)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AsignacionDocente>()
            .HasOne(a => a.Maestro)
            .WithMany(m => m.AsignacionesDocentes)
            .HasForeignKey(a => a.IdMaestro)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AsignacionDocente>()
            .HasOne(a => a.Curso)
            .WithMany(c => c.AsignacionesDocentes)
            .HasForeignKey(a => a.IdCurso)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AsignacionDocente>()
            .HasOne(a => a.Materia)
            .WithMany(m => m.AsignacionesDocentes)
            .HasForeignKey(a => a.IdMateria)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AsignacionDocente>()
            .HasOne(a => a.AnioEscolar)
            .WithMany()
            .HasForeignKey(a => a.IdAnioEscolar)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ActividadEvaluativa>()
            .HasOne(a => a.AsignacionDocente)
            .WithMany(ad => ad.ActividadesEvaluativas)
            .HasForeignKey(a => a.IdAsignacionDocente)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ActividadEvaluativa>()
            .HasOne(a => a.PeriodoPublicacion)
            .WithMany(p => p.ActividadesEvaluativas)
            .HasForeignKey(a => a.IdPeriodoPublicacion)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<NotaActividad>()
            .HasOne(n => n.ActividadEvaluativa)
            .WithMany(a => a.NotasActividades)
            .HasForeignKey(n => n.IdActividadEvaluativa)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<NotaActividad>()
            .HasOne(n => n.Estudiante)
            .WithMany()
            .HasForeignKey(n => n.IdEstudiante)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CalificacionPeriodo>()
            .HasOne(c => c.Estudiante)
            .WithMany(e => e.CalificacionesPeriodo)
            .HasForeignKey(c => c.IdEstudiante)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CalificacionPeriodo>()
            .HasOne(c => c.AsignacionDocente)
            .WithMany(a => a.CalificacionesPeriodo)
            .HasForeignKey(c => c.IdAsignacionDocente)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CalificacionPeriodo>()
            .HasOne(c => c.PeriodoPublicacion)
            .WithMany(p => p.CalificacionesPeriodo)
            .HasForeignKey(c => c.IdPeriodoPublicacion)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PadreEstudiante>()
            .HasOne(pe => pe.Padre)
            .WithMany(p => p.PadreEstudiantes)
            .HasForeignKey(pe => pe.IdPadre)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PadreEstudiante>()
            .HasOne(pe => pe.Estudiante)
            .WithMany(e => e.PadreEstudiantes)
            .HasForeignKey(pe => pe.IdEstudiante)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Observacion>()
            .HasOne(o => o.Estudiante)
            .WithMany()
            .HasForeignKey(o => o.IdEstudiante)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Observacion>()
            .HasOne(o => o.AsignacionDocente)
            .WithMany()
            .HasForeignKey(o => o.IdAsignacionDocente)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Observacion>()
            .HasOne(o => o.PeriodoPublicacion)
            .WithMany()
            .HasForeignKey(o => o.IdPeriodoPublicacion)
            .OnDelete(DeleteBehavior.Restrict);

        // =========================
        // ÍNDICES ÚNICOS
        // =========================

        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.NombreUsuario)
            .IsUnique();

        modelBuilder.Entity<Estudiante>()
            .HasIndex(e => e.Matricula)
            .IsUnique();

        modelBuilder.Entity<PadreEstudiante>()
            .HasIndex(pe => new { pe.IdPadre, pe.IdEstudiante })
            .IsUnique();

        modelBuilder.Entity<Inscripcion>()
            .HasIndex(i => new { i.IdEstudiante, i.IdAnioEscolar })
            .IsUnique();

        modelBuilder.Entity<GradoMateria>()
            .HasIndex(gm => new { gm.IdGrado, gm.IdMateria })
            .IsUnique();

        modelBuilder.Entity<AsignacionDocente>()
     .HasIndex(a => new { a.IdCurso, a.IdMateria, a.IdAnioEscolar })
     .IsUnique();

        modelBuilder.Entity<NotaActividad>()
            .HasIndex(n => new { n.IdActividadEvaluativa, n.IdEstudiante })
            .IsUnique();

        modelBuilder.Entity<CalificacionPeriodo>()
            .HasIndex(c => new { c.IdEstudiante, c.IdAsignacionDocente, c.IdPeriodoPublicacion })
            .IsUnique();

        // =========================
        // DECIMALES
        // =========================

        modelBuilder.Entity<ActividadEvaluativa>()
            .Property(a => a.Porcentaje)
            .HasColumnType("decimal(5,2)");

        modelBuilder.Entity<NotaActividad>()
            .Property(n => n.Nota)
            .HasColumnType("decimal(5,2)");

        modelBuilder.Entity<CalificacionPeriodo>()
            .Property(c => c.NotaFinal)
            .HasColumnType("decimal(5,2)");
    }
}