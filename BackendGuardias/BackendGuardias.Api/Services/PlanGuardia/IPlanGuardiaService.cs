using BackendGuardias.Api.DTOs.PlanGuardia;
using BackendGuardias.Api.Models.Views;

namespace BackendGuardias.Api.Services.PlanGuardia;

public interface IPlanGuardiaService
{
    Task<IReadOnlyList<VistaPlanGuardia>> ObtenerTodosAsync(
        int? idEstado = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null);

    Task<VistaPlanGuardia> ObtenerPorIdAsync(
        int idPlanGuardia);

    Task<VistaPlanGuardia> CrearAsync(
        PlanGuardiaCrearDto dto,
        int idPersonaActor);

    Task<VistaPlanGuardia> ActualizarAsync(
        int idPlanGuardia,
        PlanGuardiaActualizarDto dto,
        int idPersonaActor);

    Task EliminarAsync(
        int idPlanGuardia);

    Task<VistaPlanGuardia> AprobarAsync(
        int idPlanGuardia,
        int idPersonaAprobador);

    Task<VistaPlanGuardia> PlanificarAutomaticamenteAsync(
        PlanGuardiaPlanificacionAutomaticaDto dto,
        int idPersonaActor);
}