using System;

namespace BackendGuardias.Api.Models.Views;

public partial class VistaPlanGuardia
{
    public int IdPlanGuardia { get; set; }

    public DateTime FechaInicial { get; set; }

    public DateTime FechaFin { get; set; }

    public int CantidadDias { get; set; }

    public int IdEstado { get; set; }

    public string EstadoCodigo { get; set; } = null!;

    public string EstadoNombre { get; set; } = null!;

    public string? EstadoDescripcion { get; set; }

    public string? EstadoColor { get; set; }

    public string? EstadoIcono { get; set; }

    public int IdPersonaCrea { get; set; }

    public string? PersonaCreaNombre { get; set; }

    public int? IdPersonaAprobo { get; set; }

    public string? PersonaAproboNombre { get; set; }

    public DateTime? FechaHoraAprobacion { get; set; }

    public string? Observacion { get; set; }

    public long CantidadAsignaciones { get; set; }

    public long CantidadPersonas { get; set; }

    public long CantidadGrupos { get; set; }

    public long CantidadDiasAsignados { get; set; }

    public int? IdPersonaActualiza { get; set; }

    public string? PersonaActualizaNombre { get; set; }

    public DateTime FechaHoraCreado { get; set; }

    public DateTime FechaHoraActualizado { get; set; }
}