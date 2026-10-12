namespace BackendGuardias.Api.DTOs.PlanGuardia;

public sealed class PlanGuardiaCrearDto
{
    public DateTime FechaInicial { get; set; }

    public DateTime FechaFin { get; set; }

    public string? Observacion { get; set; }

    public List<DetallePlanGuardiaDto> Detalles { get; set; } = new();
}