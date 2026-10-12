namespace BackendGuardias.Api.DTOs.EstadosParte;

public sealed class EstadoParteDto
{
    public int IdEstado { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public string? Color { get; set; }

    public string? Icono { get; set; }

    public bool? Activo { get; set; }

    public int? IdPersonaCrea { get; set; }

    public int? IdPersonaActualiza { get; set; }

    public DateTime FechaHoraCreado { get; set; }

    public DateTime FechaHoraActualizado { get; set; }
}