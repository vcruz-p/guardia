namespace BackendGuardias.Api.DTOs.PlanGuardia;

public sealed class PlanGuardiaPlanificacionAutomaticaDto
{
    public int Anio { get; set; }

    public int Mes { get; set; }

    public int CantidadPersonasPorGrupoPorDia { get; set; } = 1;

    public TimeSpan? HoraInicio { get; set; }

    public TimeSpan? HoraFin { get; set; }

    public string? Observacion { get; set; }
}