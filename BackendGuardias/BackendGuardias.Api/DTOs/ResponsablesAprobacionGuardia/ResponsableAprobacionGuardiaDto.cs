namespace BackendGuardias.Api.DTOs.ResponsablesAprobacionGuardia;

public sealed class ResponsableAprobacionGuardiaDto
{
    public int IdResponsable { get; set; }

    public int IdUnidadOrganizativa { get; set; }

    public string UnidadOrganizativaNombre { get; set; } = string.Empty;

    public string? UnidadOrganizativaDescripcion { get; set; }

    public int IdPersona { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Apellidos { get; set; }

    public string? PersonaNombreCompleto { get; set; }

    public string? CarnetIdentidad { get; set; }

    public int? IdCargo { get; set; }

    public string? CargoDescripcion { get; set; }

    public int? IdGrado { get; set; }

    public string? GradoDescripcion { get; set; }

    public string Tipo { get; set; } = string.Empty;

    public int IdPersonaCrea { get; set; }

    public string? PersonaCreaNombre { get; set; }

    public DateTime FechaHoraCreado { get; set; }

    public DateTime FechaHoraActualizado { get; set; }
}