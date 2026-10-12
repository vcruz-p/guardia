using System;
using System.Collections.Generic;
using BackendGuardias.Api.Models.Entities;
using BackendGuardias.Api.Models.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BackendGuardias.Api.Data;

public partial class GuardiasDbContext : DbContext
{
    public GuardiasDbContext(
        DbContextOptions<GuardiasDbContext> options)
        : base(options)
    {
    }

    // =========================================================
    // TABLAS
    // =========================================================

    public virtual DbSet<DetallePlanGuardia> DetallePlanGuardia { get; set; }

    public virtual DbSet<EstadoParte> EstadoParte { get; set; }

    public virtual DbSet<GrupoGuardia> GrupoGuardia { get; set; }

    public virtual DbSet<GrupoGuardiaPersona> GrupoGuardiaPersona { get; set; }

    public virtual DbSet<PlanGuardia> PlanGuardia { get; set; }

    public virtual DbSet<ResponsableAprobacionGuardia> ResponsableAprobacionGuardia { get; set; }

    public virtual DbSet<CalendarioFeriado> CalendarioFeriado { get; set; }

    // =========================================================
    // VISTAS
    // =========================================================

    public virtual DbSet<VistaDetallePlanGuardia> VistaDetallePlanGuardia { get; set; }

    public virtual DbSet<VistaGrupoGuardiaPersona> VistaGrupoGuardiaPersona { get; set; }

    public virtual DbSet<VistaGruposGuardia> VistaGruposGuardia { get; set; }

    public virtual DbSet<VistaPersonasAfectaciones> VistaPersonasAfectaciones { get; set; }

    public virtual DbSet<VistaPersonasAfectacionesFuerzasMedios> VistaPersonasAfectacionesFuerzasMedios { get; set; }

    public virtual DbSet<VistaPersonasUnidadesSubordinacion> VistaPersonasUnidadesSubordinacion { get; set; }

    public virtual DbSet<VistaArbolUnidadesOrganizativas> VistaArbolUnidadesOrganizativas { get; set; }

    public virtual DbSet<VistaPlanGuardia> VistaPlanGuardia { get; set; }

    public virtual DbSet<VistaResponsableAprobacionGuardia> VistaResponsableAprobacionGuardia { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

// =========================================================
// CALENDARIO DE FERIADOS
// =========================================================

modelBuilder.Entity<CalendarioFeriado>(entity =>
{
    entity.HasKey(e => e.IdFeriado)
        .HasName("PRIMARY");

    entity.ToTable("calendario_feriado");

    entity.HasIndex(
        e => new { e.Fecha, e.Nombre, e.Ambito },
        "uk_calendario_feriado_fecha_nombre_ambito")
        .IsUnique();

    entity.HasIndex(
        e => new { e.Fecha, e.Activo },
        "idx_calendario_feriado_fecha_activo");

    entity.HasIndex(
        e => e.IdPersonaCrea,
        "idx_calendario_feriado_crea");

    entity.HasIndex(
        e => e.IdPersonaActualiza,
        "idx_calendario_feriado_actualiza");

    entity.Property(e => e.IdFeriado)
        .HasColumnName("id_feriado");

    entity.Property(e => e.Fecha)
        .HasColumnType("date")
        .HasColumnName("fecha")
        .IsRequired();

    entity.Property(e => e.Nombre)
        .HasMaxLength(150)
        .HasColumnName("nombre")
        .IsRequired();

    entity.Property(e => e.Descripcion)
        .HasMaxLength(500)
        .HasColumnName("descripcion");

    entity.Property(e => e.Ambito)
        .HasMaxLength(30)
        .HasDefaultValueSql("'NACIONAL'")
        .HasColumnName("ambito")
        .IsRequired();

    entity.Property(e => e.Activo)
        .HasDefaultValueSql("'1'")
        .HasColumnName("activo")
        .IsRequired();

    entity.Property(e => e.IdPersonaCrea)
        .HasColumnName("id_persona_crea");

    entity.Property(e => e.IdPersonaActualiza)
        .HasColumnName("id_persona_actualiza");

    entity.Property(e => e.FechaHoraCreado)
        .HasDefaultValueSql("CURRENT_TIMESTAMP")
        .HasColumnType("datetime")
        .HasColumnName("fecha_hora_creado");

    entity.Property(e => e.FechaHoraActualizado)
        .ValueGeneratedOnAddOrUpdate()
        .HasDefaultValueSql("CURRENT_TIMESTAMP")
        .HasColumnType("datetime")
        .HasColumnName("fecha_hora_actualizado");
});


        // =========================================================
        // DETALLE PLAN GUARDIA
        // =========================================================

        modelBuilder.Entity<DetallePlanGuardia>(entity =>
        {
            entity.HasKey(e => e.IdDetallePlan)
                .HasName("PRIMARY");

            entity.ToTable("detalle_plan_guardia");

            entity.HasIndex(e => e.IdPersonaActualiza,
                "idx_detalle_plan_actualiza");

            entity.HasIndex(e => e.IdPersonaCrea,
                "idx_detalle_plan_crea");

            entity.HasIndex(e => e.FechaGuardia,
                "idx_detalle_plan_fecha");

            entity.HasIndex(e => e.IdGrupoGuardia,
                "idx_detalle_plan_grupo");

            entity.HasIndex(
                e => new
                {
                    e.IdPlanGuardia,
                    e.FechaGuardia,
                    e.IdGrupoGuardia,
                    e.IdPersona
                },
                "idx_detalle_plan_guardia_busqueda");

            entity.HasIndex(e => e.IdPersona,
                "idx_detalle_plan_persona");

            entity.HasIndex(
                e => new { e.IdPersona, e.FechaGuardia },
                "idx_detalle_plan_persona_fecha");

            entity.HasIndex(e => e.IdPlanGuardia,
                "idx_detalle_plan_plan");

            entity.HasIndex(
                e => new
                {
                    e.IdPlanGuardia,
                    e.IdPersona,
                    e.FechaGuardia
                },
                "uk_detalle_plan_persona_fecha")
                .IsUnique();

            entity.Property(e => e.FechaGuardia)
                .HasColumnType("date");

            entity.Property(e => e.HoraInicio)
                .HasColumnType("time");

            entity.Property(e => e.HoraFin)
                .HasColumnType("time");

            entity.Property(e => e.Observacion)
                .HasMaxLength(500);

            entity.Property(e => e.Resultado)
                .HasMaxLength(100);

            entity.Property(e => e.FechaHoraCreado)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.Property(e => e.FechaHoraActualizado)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.HasOne(d => d.IdGrupoGuardiaNavigation)
                .WithMany(p => p.DetallePlanGuardia)
                .HasForeignKey(d => d.IdGrupoGuardia)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_detalle_plan_grupo");

            entity.HasOne(d => d.IdPlanGuardiaNavigation)
                .WithMany(p => p.DetallePlanGuardia)
                .HasForeignKey(d => d.IdPlanGuardia)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_detalle_plan_plan");
        });

        // =========================================================
        // ESTADO PARTE
        // =========================================================

        modelBuilder.Entity<EstadoParte>(entity =>
        {
            entity.HasKey(e => e.IdEstado)
                .HasName("PRIMARY");

            entity.ToTable("estado_parte");

            entity.HasIndex(e => e.Activo,
                "idx_estado_parte_activo");

            entity.HasIndex(e => e.IdPersonaActualiza,
                "idx_estado_parte_actualiza");

            entity.HasIndex(e => e.IdPersonaCrea,
                "idx_estado_parte_crea");

            entity.HasIndex(e => e.Codigo,
                "uk_estado_parte_codigo")
                .IsUnique();

            entity.HasIndex(e => e.Nombre,
                "uk_estado_parte_nombre")
                .IsUnique();

            entity.Property(e => e.Codigo)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(e => e.Descripcion)
                .HasMaxLength(500);

            entity.Property(e => e.Color)
                .HasMaxLength(50);

            entity.Property(e => e.Icono)
                .HasMaxLength(100);

            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("'1'");

            entity.Property(e => e.FechaHoraCreado)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.Property(e => e.FechaHoraActualizado)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
        });

        // =========================================================
        // GRUPO GUARDIA
        // =========================================================

        modelBuilder.Entity<GrupoGuardia>(entity =>
        {
            entity.HasKey(e => e.IdGrupoGuardia)
                .HasName("PRIMARY");

            entity.ToTable("grupo_guardia");

            entity.HasIndex(e => e.Activo,
                "idx_grupo_guardia_activo");

            entity.HasIndex(e => e.IdPersonaActualiza,
                "idx_grupo_guardia_actualiza");

            entity.HasIndex(e => e.IdPersonaCrea,
                "idx_grupo_guardia_crea");

            entity.HasIndex(e => e.SubordinadoAIdGrupoGuardia,
                "idx_grupo_guardia_padre");

            entity.HasIndex(e => e.Iniciales,
                "uk_grupo_guardia_iniciales")
                .IsUnique();

            entity.HasIndex(e => e.Nombre,
                "uk_grupo_guardia_nombre")
                .IsUnique();

            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(e => e.Iniciales)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(e => e.Color)
                .HasMaxLength(50);

            entity.Property(e => e.Icono)
                .HasMaxLength(100);

            entity.Property(e => e.Observacion)
                .HasMaxLength(500);

            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("'1'");

            entity.Property(e => e.FechaHoraCreado)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.Property(e => e.FechaHoraActualizado)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.HasOne(d => d.SubordinadoAIdGrupoGuardiaNavigation)
                .WithMany(p => p.InverseSubordinadoAIdGrupoGuardiaNavigation)
                .HasForeignKey(d => d.SubordinadoAIdGrupoGuardia)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_grupo_guardia_padre");
        });

        // =========================================================
        // GRUPO GUARDIA PERSONA
        // =========================================================

        modelBuilder.Entity<GrupoGuardiaPersona>(entity =>
        {
            entity.HasKey(e => e.IdGrupoGuardiaPersona)
                .HasName("PRIMARY");

            entity.ToTable("grupo_guardia_persona");

            entity.HasIndex(e => e.Activo,
                "idx_ggp_activo");

            entity.HasIndex(e => e.IdPersonaActualiza,
                "idx_ggp_actualiza");

            entity.HasIndex(e => e.IdPersonaCrea,
                "idx_ggp_crea");

            entity.HasIndex(e => e.IdGrupoGuardia,
                "idx_ggp_grupo");

            entity.HasIndex(
                e => new { e.FechaInicio, e.FechaFin },
                "idx_ggp_periodo");

            entity.HasIndex(e => e.IdPersona,
                "idx_ggp_persona");

            entity.HasIndex(
                e => new
                {
                    e.IdGrupoGuardia,
                    e.IdPersona,
                    e.FechaInicio,
                    e.FechaFin,
                    e.Activo
                },
                "idx_grupo_guardia_persona_busqueda");

            entity.HasIndex(
                e => new
                {
                    e.IdGrupoGuardia,
                    e.IdPersona,
                    e.FechaInicio
                },
                "uk_grupo_guardia_persona_inicio")
                .IsUnique();

            entity.Property(e => e.FechaInicio)
                .HasColumnType("date");

            entity.Property(e => e.FechaFin)
                .HasColumnType("date");

            entity.Property(e => e.Observacion)
                .HasMaxLength(500);

            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("'1'");

            entity.Property(e => e.FechaHoraCreado)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.Property(e => e.FechaHoraActualizado)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.HasOne(d => d.IdGrupoGuardiaNavigation)
                .WithMany(p => p.GrupoGuardiaPersona)
                .HasForeignKey(d => d.IdGrupoGuardia)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_ggp_grupo");
        });

        // =========================================================
        // PLAN GUARDIA
        // =========================================================

       modelBuilder.Entity<PlanGuardia>(entity =>
{
    entity.HasKey(e => e.IdPlanGuardia)
        .HasName("PRIMARY");

    entity.ToTable("plan_guardia");

    entity.HasIndex(
        e => e.IdPersonaActualiza,
        "idx_plan_guardia_actualiza");

    entity.HasIndex(
        e => e.IdPersonaAprobo,
        "idx_plan_guardia_aprobo");

    entity.HasIndex(
        e => e.IdPersonaCrea,
        "idx_plan_guardia_crea");

    entity.HasIndex(
        e => e.IdEstado,
        "idx_plan_guardia_estado");

    entity.HasIndex(
        e => new { e.FechaInicial, e.FechaFin },
        "idx_plan_guardia_fecha");

    // Índice compuesto definido en la tabla original.
    entity.HasIndex(
        e => new
        {
            e.IdEstado,
            e.FechaInicial,
            e.FechaFin
        },
        "idx_plan_guardia_unidad_estado_fecha");

    entity.Property(e => e.FechaInicial)
        .HasColumnType("date")
        .IsRequired();

    entity.Property(e => e.FechaFin)
        .HasColumnType("date")
        .IsRequired();

    entity.Property(e => e.Observacion)
        .HasMaxLength(1000);

    entity.Property(e => e.FechaHoraAprobacion)
        .HasColumnType("datetime");

    entity.Property(e => e.FechaHoraCreado)
        .HasDefaultValueSql("CURRENT_TIMESTAMP")
        .HasColumnType("datetime");

    entity.Property(e => e.FechaHoraActualizado)
        .ValueGeneratedOnAddOrUpdate()
        .HasDefaultValueSql("CURRENT_TIMESTAMP")
        .HasColumnType("datetime");

    entity.HasOne(d => d.IdEstadoNavigation)
        .WithMany(p => p.PlanGuardia)
        .HasForeignKey(d => d.IdEstado)
        .OnDelete(DeleteBehavior.Restrict)
        .HasConstraintName("fk_plan_guardia_estado");
});

        // =========================================================
        // RESPONSABLE APROBACIÓN GUARDIA
        // =========================================================

        modelBuilder.Entity<ResponsableAprobacionGuardia>(entity =>
        {
            entity.HasKey(e => e.IdResponsable)
                .HasName("PRIMARY");

            entity.ToTable("responsable_aprobacion_guardia");

            entity.HasIndex(e => e.IdPersona,
                "idx_responsable_persona");

            entity.HasIndex(e => e.IdPersonaCrea,
                "idx_responsable_persona_crea");

            entity.HasIndex(
                e => new { e.IdUnidadOrganizativa, e.Tipo },
                "uk_responsable_unidad_tipo")
                .IsUnique();

            entity.Property(e => e.Tipo)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.FechaHoraCreado)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.Property(e => e.FechaHoraActualizado)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
        });

        // =========================================================
        // VISTA DETALLE PLAN GUARDIA
        //
        // Debe coincidir con el SQL que devuelve:
        // - datos del plan
        // - grupo de guardia
        // - persona y datos profesionales
        // - auditoría
        // - indicador de afectación
        //
        // No incluye Resultado ni datos de unidad organizativa
        // porque no los devuelve la versión de la vista definida.
        // =========================================================

        modelBuilder.Entity<VistaDetallePlanGuardia>(entity =>
        {
            entity.HasNoKey()
                .ToView("vista_detalle_plan_guardia");

            entity.Property(e => e.IdDetallePlan)
                .HasColumnName("id_detalle_plan");

            entity.Property(e => e.IdPlanGuardia)
                .HasColumnName("id_plan_guardia");

            entity.Property(e => e.PlanFechaInicial)
                .HasColumnType("date")
                .HasColumnName("plan_fecha_inicial");

            entity.Property(e => e.PlanFechaFin)
                .HasColumnType("date")
                .HasColumnName("plan_fecha_fin");

            entity.Property(e => e.FechaGuardia)
                .HasColumnType("date")
                .HasColumnName("fecha_guardia");

            entity.Property(e => e.HoraInicio)
                .HasColumnType("time")
                .HasColumnName("hora_inicio");

            entity.Property(e => e.HoraFin)
                .HasColumnType("time")
                .HasColumnName("hora_fin");

            entity.Property(e => e.Observacion)
                .HasMaxLength(500)
                .HasColumnName("observacion");

            entity.Property(e => e.IdGrupoGuardia)
                .HasColumnName("id_grupo_guardia");

            entity.Property(e => e.GrupoNombre)
                .HasMaxLength(150)
                .HasColumnName("grupo_nombre");

            entity.Property(e => e.GrupoIniciales)
                .HasMaxLength(30)
                .HasColumnName("grupo_iniciales");

            entity.Property(e => e.IdPersona)
                .HasColumnName("id_persona");

            entity.Property(e => e.Nombre)
                .HasColumnName("nombre");

            entity.Property(e => e.Apellidos)
                .HasColumnName("apellidos");

            entity.Property(e => e.PersonaNombreCompleto)
                .HasColumnName("persona_nombre_completo");

            entity.Property(e => e.CarnetIdentidad)
                .HasColumnName("carnet_identidad");

            entity.Property(e => e.IdCargo)
                .HasColumnName("id_cargo");

            entity.Property(e => e.CargoDescripcion)
                .HasColumnName("cargo_descripcion");

            entity.Property(e => e.IdGrado)
                .HasColumnName("id_grado");

            entity.Property(e => e.GradoDescripcion)
                .HasColumnName("grado_descripcion");

            entity.Property(e => e.Sexo)
                .HasColumnName("sexo");

            entity.Property(e => e.IdPersonaCrea)
                .HasColumnName("id_persona_crea");

            entity.Property(e => e.PersonaCreaNombre)
                .HasColumnName("persona_crea_nombre");

            entity.Property(e => e.IdPersonaActualiza)
                .HasColumnName("id_persona_actualiza");

            entity.Property(e => e.PersonaActualizaNombre)
                .HasColumnName("persona_actualiza_nombre");

            entity.Property(e => e.FechaHoraCreado)
                .HasColumnType("datetime")
                .HasColumnName("fecha_hora_creado");

            entity.Property(e => e.FechaHoraActualizado)
                .HasColumnType("datetime")
                .HasColumnName("fecha_hora_actualizado");

            entity.Property(e => e.TieneAfectacionFuerzasMedios)
                .HasColumnName("tiene_afectacion_fuerzas_medios");
        });

        // =========================================================
        // VISTA GRUPO GUARDIA PERSONA
        // =========================================================

        modelBuilder.Entity<VistaGrupoGuardiaPersona>(entity =>
        {
            entity.HasNoKey()
                .ToView("vista_grupo_guardia_persona");

            entity.Property(e => e.FechaInicio)
                .HasColumnType("date");

            entity.Property(e => e.FechaFin)
                .HasColumnType("date");

            entity.Property(e => e.FechaHoraCreado)
                .HasColumnType("datetime");

            entity.Property(e => e.FechaHoraActualizado)
                .HasColumnType("datetime");
        });

        // =========================================================
        // VISTA GRUPOS GUARDIA
        // =========================================================

        modelBuilder.Entity<VistaGruposGuardia>(entity =>
        {
            entity.HasNoKey()
                .ToView("vista_grupos_guardia");

            entity.Property(e => e.FechaHoraCreado)
                .HasColumnType("datetime");

            entity.Property(e => e.FechaHoraActualizado)
                .HasColumnType("datetime");
        });

        // =========================================================
        // VISTA PERSONAS AFECTACIONES
        // =========================================================

        modelBuilder.Entity<VistaPersonasAfectaciones>(entity =>
        {
            entity.HasNoKey()
                .ToView("vista_personas_afectaciones");

            entity.Property(e => e.FechaHoraRegistro)
                .HasColumnType("timestamp");

            entity.Property(e => e.FechaHoraInicio)
                .HasColumnType("datetime");

            entity.Property(e => e.FechaHoraFin)
                .HasColumnType("datetime");
        });

        // =========================================================
        // VISTA PERSONAS AFECTACIONES FUERZAS MEDIOS
        // =========================================================

        modelBuilder.Entity<VistaPersonasAfectacionesFuerzasMedios>(entity =>
        {
            entity.HasNoKey()
                .ToView("vista_personas_afectaciones_fuerzas_medios");

            entity.Property(e => e.FechaHoraRegistro)
                .HasColumnType("timestamp");

            entity.Property(e => e.FechaHoraInicio)
                .HasColumnType("datetime");

            entity.Property(e => e.FechaHoraFin)
                .HasColumnType("datetime");
        });

        // =========================================================
        // VISTA PERSONAS UNIDADES SUBORDINACIÓN
        // =========================================================

        modelBuilder.Entity<VistaPersonasUnidadesSubordinacion>(entity =>
        {
            entity.HasNoKey()
                .ToView("vista_personas_unidades_subordinacion");

            entity.Property(e => e.IdsUnidadesSuperiores)
                .HasColumnType("text");

            entity.Property(e => e.UnidadesSuperiores)
                .HasColumnType("text");

            entity.Property(e => e.UnidadesSuperioresDescripcion)
                .HasColumnType("text");
        });

        // =========================================================
        // VISTA ÁRBOL DE UNIDADES ORGANIZATIVAS
        // =========================================================

        modelBuilder.Entity<VistaArbolUnidadesOrganizativas>(entity =>
        {
            entity.HasNoKey()
                .ToView("vista_arbol_unidades_organizativas");

            entity.Property(e => e.IdsUnidadesSuperiores)
                .HasColumnType("text");
        });

        // =========================================================
        // VISTA PLAN GUARDIA
        //
        // Modelo ajustado a las columnas de la vista definida:
        // no contiene UnidadOrganizativa, ResponsableAprueba
        // ni CausaRechazo.
        // =========================================================

        modelBuilder.Entity<VistaPlanGuardia>(entity =>
        {
            entity.HasNoKey()
                .ToView("vista_plan_guardia");

            entity.Property(e => e.IdPlanGuardia)
                .HasColumnName("id_plan_guardia");

            entity.Property(e => e.FechaInicial)
                .HasColumnType("date")
                .HasColumnName("fecha_inicial");

            entity.Property(e => e.FechaFin)
                .HasColumnType("date")
                .HasColumnName("fecha_fin");

            entity.Property(e => e.CantidadDias)
                .HasColumnName("cantidad_dias");

            entity.Property(e => e.IdEstado)
                .HasColumnName("id_estado");

            entity.Property(e => e.EstadoCodigo)
                .HasColumnName("estado_codigo");

            entity.Property(e => e.EstadoNombre)
                .HasColumnName("estado_nombre");

            entity.Property(e => e.EstadoDescripcion)
                .HasColumnName("estado_descripcion");

            entity.Property(e => e.EstadoColor)
                .HasColumnName("estado_color");

            entity.Property(e => e.EstadoIcono)
                .HasColumnName("estado_icono");

            entity.Property(e => e.IdPersonaCrea)
                .HasColumnName("id_persona_crea");

            entity.Property(e => e.PersonaCreaNombre)
                .HasColumnName("persona_crea_nombre");

            entity.Property(e => e.IdPersonaAprobo)
                .HasColumnName("id_persona_aprobo");

            entity.Property(e => e.PersonaAproboNombre)
                .HasColumnName("persona_aprobo_nombre");

            entity.Property(e => e.FechaHoraAprobacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_hora_aprobacion");

            entity.Property(e => e.Observacion)
                .HasColumnName("observacion");

            entity.Property(e => e.CantidadAsignaciones)
                .HasColumnName("cantidad_asignaciones");

            entity.Property(e => e.CantidadPersonas)
                .HasColumnName("cantidad_personas");

            entity.Property(e => e.CantidadGrupos)
                .HasColumnName("cantidad_grupos");

            entity.Property(e => e.CantidadDiasAsignados)
                .HasColumnName("cantidad_dias_asignados");

            entity.Property(e => e.IdPersonaActualiza)
                .HasColumnName("id_persona_actualiza");

            entity.Property(e => e.PersonaActualizaNombre)
                .HasColumnName("persona_actualiza_nombre");

            entity.Property(e => e.FechaHoraCreado)
                .HasColumnType("datetime")
                .HasColumnName("fecha_hora_creado");

            entity.Property(e => e.FechaHoraActualizado)
                .HasColumnType("datetime")
                .HasColumnName("fecha_hora_actualizado");
        });

        // =========================================================
        // VISTA RESPONSABLE APROBACIÓN GUARDIA
        // =========================================================

        modelBuilder.Entity<VistaResponsableAprobacionGuardia>(entity =>
        {
            entity.HasNoKey()
                .ToView("vista_responsable_aprobacion_guardia");

            entity.Property(e => e.FechaHoraCreado)
                .HasColumnType("datetime");

            entity.Property(e => e.FechaHoraActualizado)
                .HasColumnType("datetime");
        });

        // =========================================================
        // CONVENCIÓN PARA NOMBRES DE COLUMNAS
        //
        // Las propiedades que no tengan HasColumnName explícito
        // se asignan a su equivalente snake_case:
        // IdGrupoGuardia -> id_grupo_guardia.
        //
        // Esto permite mantener correctamente las columnas de
        // todas las vistas sin repetir la misma configuración.
        // =========================================================

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));
            }
        }

        OnModelCreatingPartial(modelBuilder);
    }

    private static string ToSnakeCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        var result = new System.Text.StringBuilder(value.Length + 10);

        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];

            if (char.IsUpper(c))
            {
                if (i > 0)
                    result.Append('_');

                result.Append(char.ToLowerInvariant(c));
            }
            else
            {
                result.Append(c);
            }
        }

        return result.ToString();
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}