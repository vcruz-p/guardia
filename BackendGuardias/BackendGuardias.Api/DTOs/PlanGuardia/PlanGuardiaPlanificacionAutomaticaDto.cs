namespace BackendGuardias.Api.DTOs.PlanGuardia;

/// <summary>
/// Entrada para la planificación automática de guardias.
/// Recibe la fecha inicial y la fecha final del período a planificar.
/// </summary>
public sealed class PlanGuardiaPlanificacionAutomaticaDto
{
    /// <summary>
    /// Fecha inicial del período a planificar (obligatoria).
    /// </summary>
    public DateTime FechaInicial { get; set; }

    /// <summary>
    /// Fecha final del período a planificar (obligatoria,
    /// inclusiva: ese día también se planifica).
    /// </summary>
    public DateTime FechaFin { get; set; }

    public int CantidadPersonasPorGrupoPorDia { get; set; } = 1;

    public TimeSpan? HoraInicio { get; set; }

    public TimeSpan? HoraFin { get; set; }

    public string? Observacion { get; set; }
}
