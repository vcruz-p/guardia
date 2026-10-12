using System.ComponentModel.DataAnnotations;

namespace BackendGuardias.Api.DTOs.EstadosParte;

public sealed class CrearEstadoParteDto
{
    [Required(ErrorMessage = "El código es obligatorio.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public string? Color { get; set; }

    public string? Icono { get; set; }

    public bool? Activo { get; set; } = true;
}