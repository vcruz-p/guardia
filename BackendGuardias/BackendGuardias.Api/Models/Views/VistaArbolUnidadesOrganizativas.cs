namespace BackendGuardias.Api.Models.Views;

public sealed class VistaArbolUnidadesOrganizativas
{
    public int IdUnidadOrganizativa { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int? IdCategoria { get; set; }
    public string? CategoriaNombre { get; set; }
    public string? CategoriaDescripcion { get; set; }
    public string? IdsUnidadesSuperiores { get; set; }
    public string? UnidadesSuperiores { get; set; }
    public string? UnidadesSuperioresDescripcion { get; set; }
}
