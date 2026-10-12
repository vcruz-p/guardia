using System.Text.Json.Serialization;
using BackendGuardias.Api.Models.Views;

namespace BackendGuardias.Api.DTOs.PersonasUnidadOrganizativa;

public sealed class UnidadOrganizativaArbolDto
{
    public int IdUnidadOrganizativa { get; set; }

    public int? IdUnidadOrganizativaPadre { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }
    public int? IdCategoria { get; set; }
    public string? CategoriaNombre { get; set; }
    public string? CategoriaDescripcion { get; set; }
    public string? UnidadesSuperiores { get; set; }
    public string? UnidadesSuperioresDescripcion { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<VistaPersonasUnidadesSubordinacion>? Personas { get; set; }

    public List<UnidadOrganizativaArbolDto> UnidadesHijas { get; set; } = [];
}