namespace BackendGuardias.Api.Models.Views;

public sealed class VistaArbolUnidadesOrganizativas
{
    public int IdUnidadOrganizativa { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public string? IdsUnidadesSuperiores { get; set; }
}