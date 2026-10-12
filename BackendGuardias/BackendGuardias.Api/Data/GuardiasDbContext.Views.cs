using BackendGuardias.Api.Models.Views;
using Microsoft.EntityFrameworkCore;

namespace BackendGuardias.Api.Data;

public partial class GuardiasDbContext
{
    /// <summary>
    /// Personas con su unidad organizativa y sus unidades superiores.
    /// Esta vista es exclusivamente de lectura.
    /// </summary>
    public DbSet<PersonaUnidadSubordinacionView>
        PersonasUnidadOrganizativa =>
            Set<PersonaUnidadSubordinacionView>();

    /// <summary>
    /// Configuración adicional de las vistas del módulo.
    /// Se invoca desde OnModelCreating del contexto principal.
    /// </summary>
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PersonaUnidadSubordinacionView>(entity =>
        {
            entity.HasNoKey();

            entity.ToView(
                "vista_personas_unidades_subordinacion",
                "guardia"
            );

            entity.Property(e => e.IdPersona)
                .HasColumnName("id_persona");

            entity.Property(e => e.Nombre)
                .HasColumnName("nombre");

            entity.Property(e => e.Apellidos)
                .HasColumnName("apellidos");

            entity.Property(e => e.NombreCompleto)
                .HasColumnName("nombre_completo");

            entity.Property(e => e.CarnetIdentidad)
                .HasColumnName("carnet_identidad");

            entity.Property(e => e.IdSexo).HasColumnName("id_sexo");
            entity.Property(e => e.SexoNombre).HasColumnName("sexo_nombre");
            entity.Property(e => e.SexoDescripcion).HasColumnName("sexo_descripcion");
            entity.Property(e => e.IdColorPiel).HasColumnName("id_color_piel");
            entity.Property(e => e.ColorPielNombre).HasColumnName("color_piel_nombre");
            entity.Property(e => e.ColorPielDescripcion).HasColumnName("color_piel_descripcion");
            entity.Property(e => e.IdPersonaCargo).HasColumnName("id_persona_cargo");
            entity.Property(e => e.IdCargoUnidadOrganizativa).HasColumnName("id_cargo_unidad_organizativa");
            entity.Property(e => e.CargoNombre).HasColumnName("cargo_nombre");
            entity.Property(e => e.CargoFechaIntroduccion)
                .HasColumnType("timestamp")
                .HasColumnName("cargo_fecha_introduccion");
            entity.Property(e => e.CargoFechaFin)
                .HasColumnType("timestamp")
                .HasColumnName("cargo_fecha_fin");
            entity.Property(e => e.CargoObservacion).HasColumnName("cargo_observacion");
            entity.Property(e => e.IdCategoriaUnidadOrganizativa)
                .HasColumnName("id_categoria_unidad_organizativa");
            entity.Property(e => e.CategoriaUnidadOrganizativaNombre)
                .HasColumnName("categoria_unidad_organizativa_nombre");
            entity.Property(e => e.CategoriaUnidadOrganizativaDescripcion)
                .HasColumnName("categoria_unidad_organizativa_descripcion");
            entity.Property(e => e.PersonaGradoId).HasColumnName("persona_grado_id");
            entity.Property(e => e.GradoNombre).HasColumnName("grado_nombre");
            entity.Property(e => e.GradoFechaInsercion)
                .HasColumnType("timestamp")
                .HasColumnName("grado_fecha_insercion");
            entity.Property(e => e.GradoFechaFin)
                .HasColumnType("timestamp")
                .HasColumnName("grado_fecha_fin");
            entity.Property(e => e.GradoObservacion).HasColumnName("grado_observacion");
            entity.Property(e => e.IdPersonaInserta).HasColumnName("id_persona_inserta");
            entity.Property(e => e.PersonaInsertaNombre).HasColumnName("persona_inserta_nombre");
            entity.Property(e => e.NombreUsuario).HasColumnName("nombre_usuario");
            entity.Property(e => e.TieneUnidadSuperior).HasColumnName("tiene_unidad_superior");
            entity.Property(e => e.CantidadUnidadesSuperiores)
                .HasColumnName("cantidad_unidades_superiores");

            entity.Property(e => e.IdUnidadOrganizativa)
                .HasColumnName("id_unidad_organizativa");

            entity.Property(e => e.UnidadOrganizativaNombre)
                .HasColumnName("unidad_organizativa_nombre");

            entity.Property(e => e.UnidadOrganizativaDescripcion)
                .HasColumnName("unidad_organizativa_descripcion");

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

            entity.Property(e => e.IdsUnidadesSuperiores)
                .HasColumnName("ids_unidades_superiores");

            entity.Property(e => e.UnidadesSuperiores)
                .HasColumnName("unidades_superiores");

            entity.Property(e => e.UnidadesSuperioresDescripcion)
                .HasColumnName("unidades_superiores_descripcion");
        });
    }
}