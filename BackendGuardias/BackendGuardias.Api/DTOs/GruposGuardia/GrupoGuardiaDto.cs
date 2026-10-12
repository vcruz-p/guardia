namespace BackendGuardias.Api.DTOs.GruposGuardia;

public class GrupoGuardiaDto
{
    public int IdGrupoGuardia { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Iniciales { get; set; } = string.Empty;

    public int? SubordinadoAIdGrupoGuardia { get; set; }
    public string? GrupoPadreNombre { get; set; }
    public string? GrupoPadreIniciales { get; set; }

    public string? Color { get; set; }

    public string? Icono { get; set; }

    public string? Observacion { get; set; }

    public bool Activo { get; set; }
    public long CantidadPersonas { get; set; }

    public int? IdPersonaCrea { get; set; }

    public int? IdPersonaActualiza { get; set; }
    public string? PersonaCreaNombre { get; set; }
    public string? PersonaActualizaNombre { get; set; }

    public DateTime FechaHoraCreado { get; set; }

    public DateTime FechaHoraActualizado { get; set; }
}