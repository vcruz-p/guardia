namespace BackendGuardias.Api.Models.Views;

public partial class VistaResponsableAprobacionGuardia
{
    public int IdResponsable { get; set; }

    public int IdUnidadOrganizativa { get; set; }

    public string UnidadOrganizativaNombre { get; set; } = null!;

    public string? UnidadOrganizativaDescripcion { get; set; }

    public int IdPersona { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Apellidos { get; set; }

    public string? PersonaNombreCompleto { get; set; }

    public string? CarnetIdentidad { get; set; }

    public int? IdCargo { get; set; }

    public string? CargoDescripcion { get; set; }

    public int? IdGrado { get; set; }

    public string? GradoDescripcion { get; set; }

    public string Tipo { get; set; } = null!;

    public int IdPersonaCrea { get; set; }

    public string? PersonaCreaNombre { get; set; }

    public DateTime FechaHoraCreado { get; set; }

    public DateTime FechaHoraActualizado { get; set; }
}