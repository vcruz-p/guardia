using System.ComponentModel.DataAnnotations;

namespace BackendGuardias.Api.DTOs.GruposGuardia;

public class ActualizarGrupoGuardiaDto
{
    [Required, StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string Iniciales { get; set; } = string.Empty;

    public int? IdPersonaActualiza { get; set; }

    public int? SubordinadoAIdGrupoGuardia { get; set; }

    [StringLength(50)]
    public string? Color { get; set; }

    [StringLength(100)]
    public string? Icono { get; set; }

    [StringLength(500)]
    public string? Observacion { get; set; }

    public bool Activo { get; set; }
}