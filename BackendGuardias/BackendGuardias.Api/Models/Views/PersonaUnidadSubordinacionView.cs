namespace BackendGuardias.Api.Models.Views;

public sealed class PersonaUnidadSubordinacionView
{
    public int IdPersona { get; set; }

    public string? Nombre { get; set; }

    public string? Apellidos { get; set; }

    public string? NombreCompleto { get; set; }

    public string? CarnetIdentidad { get; set; }

    public int? IdSexo { get; set; }
    public string? SexoNombre { get; set; }
    public string? SexoDescripcion { get; set; }

    public int? IdColorPiel { get; set; }
    public string? ColorPielNombre { get; set; }
    public string? ColorPielDescripcion { get; set; }

    public int? IdPersonaCargo { get; set; }
    public int? IdCargoUnidadOrganizativa { get; set; }
    public string? CargoNombre { get; set; }
    public DateTime? CargoFechaIntroduccion { get; set; }
    public DateTime? CargoFechaFin { get; set; }
    public string? CargoObservacion { get; set; }

    public int? IdCategoriaUnidadOrganizativa { get; set; }
    public string? CategoriaUnidadOrganizativaNombre { get; set; }
    public string? CategoriaUnidadOrganizativaDescripcion { get; set; }

    public int? PersonaGradoId { get; set; }
    public string? GradoNombre { get; set; }
    public DateTime? GradoFechaInsercion { get; set; }
    public DateTime? GradoFechaFin { get; set; }
    public string? GradoObservacion { get; set; }

    public int? IdPersonaInserta { get; set; }
    public string? PersonaInsertaNombre { get; set; }
    public string? NombreUsuario { get; set; }
    public int? TieneUnidadSuperior { get; set; }
    public int? CantidadUnidadesSuperiores { get; set; }

    public int? IdUnidadOrganizativa { get; set; }

    public string? UnidadOrganizativaNombre { get; set; }

    public string? UnidadOrganizativaDescripcion { get; set; }

    public int? IdCargo { get; set; }

    public string? CargoDescripcion { get; set; }
    public string? CargoNombre { get; set; }

    public int? IdGrado { get; set; }

    public string? GradoDescripcion { get; set; }
    public string? GradoNombre { get; set; }

    /// <summary>
    /// Sexo de la persona (p. ej. "Masculino" / "Femenino"),
    /// traído desde la base `persona` por la vista de la BD.
    /// </summary>
    public string? Sexo { get; set; }

    public string? IdsUnidadesSuperiores { get; set; }

    public string? UnidadesSuperiores { get; set; }

    public string? UnidadesSuperioresDescripcion { get; set; }
}