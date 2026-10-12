using System;

namespace BackendGuardias.Api.Models.Entities;

public partial class DetallePlanGuardia
{
    public int IdDetallePlan { get; set; }

    public int IdPlanGuardia { get; set; }

    public int IdGrupoGuardia { get; set; }

    public int IdPersona { get; set; }

    public DateTime FechaGuardia { get; set; }

    public TimeSpan? HoraInicio { get; set; }

    public TimeSpan? HoraFin { get; set; }
    public string? Observacion { get; set; }

    public int? IdPersonaCrea { get; set; }

    public int? IdPersonaActualiza { get; set; }

    public DateTime FechaHoraCreado { get; set; }

    public DateTime FechaHoraActualizado { get; set; }

    // Relación con el grupo de guardia.
    public virtual GrupoGuardia IdGrupoGuardiaNavigation
    {
        get;
        set;
    } = null!;

    // Relación con el plan al que pertenece.
    public virtual PlanGuardia IdPlanGuardiaNavigation
    {
        get;
        set;
    } = null!;
}