namespace BackendGuardias.Api.DTOs.PlanGuardia;

public sealed class DetallePlanGuardiaDto
{
    public int IdGrupoGuardia { get; set; }

    public int IdPersona { get; set; }

    public DateTime FechaGuardia { get; set; }

    public TimeSpan? HoraInicio { get; set; }

    public TimeSpan? HoraFin { get; set; }

    public string? Observacion { get; set; }
}