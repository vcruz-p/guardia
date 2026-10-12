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

            entity.Property(e => e.IdsUnidadesSuperiores)
                .HasColumnName("ids_unidades_superiores");

            entity.Property(e => e.UnidadesSuperiores)
                .HasColumnName("unidades_superiores");

            entity.Property(e => e.UnidadesSuperioresDescripcion)
                .HasColumnName("unidades_superiores_descripcion");
        });
    }
}