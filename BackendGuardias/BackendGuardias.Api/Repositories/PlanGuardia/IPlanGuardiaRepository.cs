using BackendGuardias.Api.DTOs.PlanGuardia;
using BackendGuardias.Api.Models.Entities;
using BackendGuardias.Api.Models.Views;

using PlanGuardiaEntity =
    BackendGuardias.Api.Models.Entities.PlanGuardia;

namespace BackendGuardias.Api.Repositories.PlanGuardia;

public interface IPlanGuardiaRepository
{
    // =========================================================
    // CONSULTAS DE PLANES
    // =========================================================

    Task<IReadOnlyList<VistaPlanGuardia>> ObtenerTodosAsync(
        int? idEstado = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null);

    Task<VistaPlanGuardia?> ObtenerVistaPorIdAsync(
        int idPlanGuardia);

    Task<PlanGuardiaEntity?> ObtenerEntidadPorIdAsync(
        int idPlanGuardia);

    // =========================================================
    // VALIDACIONES
    // =========================================================

    Task<bool> ExisteSolapamientoAsync(
        DateTime fechaInicial,
        DateTime fechaFin,
        int? excluirIdPlanGuardia = null);

    /// <summary>
    /// Devuelve los días concretos del período que ya están
    /// cubiertos por planes de guardia existentes. Se usa para
    /// dar un mensaje de error preciso en la planificación
    /// automática cuando algún día ya tiene plan.
    /// </summary>
    Task<IReadOnlyList<DateTime>> ObtenerFechasSolapadasAsync(
        DateTime fechaInicial,
        DateTime fechaFin);

    Task<bool> EsResponsableAprobacionAsync(
        int idPersona);

    // =========================================================
    // ESTADOS
    // =========================================================

    Task<EstadoParte> ObtenerOCrearEstadoAsync(
        string codigo,
        string nombre,
        string descripcion,
        string color,
        string icono,
        int idPersona);

    // =========================================================
    // OPERACIONES CRUD
    // =========================================================

    Task CrearAsync(PlanGuardiaEntity plan);

    Task GuardarCambiosAsync();

    Task EliminarAsync(PlanGuardiaEntity plan);

    // =========================================================
    // INTEGRANTES DE GRUPOS DE GUARDIA
    // =========================================================

    Task<IReadOnlyList<VistaGrupoGuardiaPersona>> ObtenerMiembrosGrupoPeriodoAsync(
            DateTime fechaInicial,
            DateTime fechaFin);

    // =========================================================
    // AFECTACIONES DE PERSONAS
    // =========================================================

    Task<IReadOnlyList<VistaPersonasAfectaciones>> ObtenerAfectacionesPeriodoAsync(
            IReadOnlyCollection<int> idsPersonas,
            DateTime fechaInicial,
            DateTime fechaFin);

    // =========================================================
    // HISTORIAL DE GUARDIAS APROBADAS
    // =========================================================

    Task<IReadOnlyList<RegistroGuardiaHistoricaDto>> ObtenerHistorialAprobadoAsync(
            DateTime fechaInicial,
            DateTime fechaFin);


     Task<IReadOnlyList<CalendarioFeriado>> ObtenerFeriadosPeriodoAsync(
    DateTime fechaInicial,
    DateTime fechaFin);       
}