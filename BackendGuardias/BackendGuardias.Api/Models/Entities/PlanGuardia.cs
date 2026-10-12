using System;
using System.Collections.Generic;

namespace BackendGuardias.Api.Models.Entities;

public partial class PlanGuardia
{
    public int IdPlanGuardia { get; set; }

    public DateTime FechaInicial { get; set; }

    public DateTime FechaFin { get; set; }

    public int IdEstado { get; set; }

    public int IdPersonaCrea { get; set; }

    public int? IdPersonaAprobo { get; set; }

    public DateTime? FechaHoraAprobacion { get; set; }

    public string? Observacion { get; set; }

    public int? IdPersonaActualiza { get; set; }

    public DateTime FechaHoraCreado { get; set; }

    public DateTime FechaHoraActualizado { get; set; }

    // Relación: un plan tiene múltiples asignaciones.
    public virtual ICollection<DetallePlanGuardia> DetallePlanGuardia
    {
        get;
        set;
    } = new List<DetallePlanGuardia>();

    // Relación con estado_parte.
    public virtual EstadoParte IdEstadoNavigation { get; set; } = null!;
}