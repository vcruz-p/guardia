using System.ComponentModel.DataAnnotations;

namespace BackendGuardias.Api.DTOs.ResponsablesAprobacionGuardia;

public sealed class CrearResponsableAprobacionGuardiaDto
{
    [Range(1, int.MaxValue,
        ErrorMessage = "Debe seleccionar una persona válida.")]
    public int IdPersona { get; set; }

    [Range(1, int.MaxValue,
        ErrorMessage = "Debe seleccionar una unidad organizativa válida.")]
    public int IdUnidadOrganizativa { get; set; }
}