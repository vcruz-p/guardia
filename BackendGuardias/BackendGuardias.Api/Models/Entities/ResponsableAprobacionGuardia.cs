namespace BackendGuardias.Api.Models.Entities;

public partial class ResponsableAprobacionGuardia
{
    public int IdResponsable { get; set; }

    public int IdPersona { get; set; }

    public int IdUnidadOrganizativa { get; set; }

    public string Tipo { get; set; } = null!;

    public int IdPersonaCrea { get; set; }

    public DateTime FechaHoraCreado { get; set; }

    public DateTime FechaHoraActualizado { get; set; }
}