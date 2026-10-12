using System.ComponentModel.DataAnnotations;

namespace BackendGuardias.Api.DTOs.GruposGuardia;

public sealed class AsociarPersonaGrupoGuardiaDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El idPersona debe ser mayor que cero.")]
    public int IdPersona { get; set; }

    public DateOnly? FechaInicio { get; set; }

    [StringLength(500, ErrorMessage = "La observación no puede superar 500 caracteres.")]
    public string? Observacion { get; set; }
}