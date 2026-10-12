namespace BackendGuardias.Api.Models.Entities;

public partial class CalendarioFeriado
{
    public int IdFeriado { get; set; }

    public DateTime Fecha { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string Ambito { get; set; } = "NACIONAL";

    public bool Activo { get; set; } = true;

    public int IdPersonaCrea { get; set; }

    public int? IdPersonaActualiza { get; set; }

    public DateTime FechaHoraCreado { get; set; }

    public DateTime FechaHoraActualizado { get; set; }
}