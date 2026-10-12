using BackendGuardias.Api.Data;
using BackendGuardias.Api.DTOs.PlanGuardia;
using BackendGuardias.Api.Models.Entities;
using BackendGuardias.Api.Models.Views;
using Microsoft.EntityFrameworkCore;

using PlanGuardiaEntity =
    BackendGuardias.Api.Models.Entities.PlanGuardia;

namespace BackendGuardias.Api.Repositories.PlanGuardia;

public sealed class PlanGuardiaRepository : IPlanGuardiaRepository
{
    private readonly GuardiasDbContext _context;

    public PlanGuardiaRepository(GuardiasDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // CONSULTAR TODOS LOS PLANES
    // =========================================================

    public async Task<IReadOnlyList<VistaPlanGuardia>> ObtenerTodosAsync(
        int? idEstado = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null)
    {
        var planes = _context.PlanGuardia
            .AsNoTracking()
            .AsQueryable();

        if (idEstado.HasValue)
        {
            planes = planes.Where(p =>
                p.IdEstado == idEstado.Value);
        }

        if (fechaDesde.HasValue)
        {
            planes = planes.Where(p =>
                p.FechaFin >= fechaDesde.Value.Date);
        }

        if (fechaHasta.HasValue)
        {
            planes = planes.Where(p =>
                p.FechaInicial <= fechaHasta.Value.Date);
        }

        var idsPlanes = planes.Select(p => p.IdPlanGuardia);

        return await _context.VistaPlanGuardia
            .AsNoTracking()
            .Where(v => idsPlanes.Contains(v.IdPlanGuardia))
            .OrderByDescending(v => v.FechaInicial)
            .ThenByDescending(v => v.IdPlanGuardia)
            .ToListAsync();
    }

    // =========================================================
    // CONSULTAR VISTA POR ID
    // =========================================================

    public async Task<VistaPlanGuardia?> ObtenerVistaPorIdAsync(
        int idPlanGuardia)
    {
        return await _context.VistaPlanGuardia
            .AsNoTracking()
            .FirstOrDefaultAsync(p =>
                p.IdPlanGuardia == idPlanGuardia);
    }

    // =========================================================
    // OBTENER ENTIDAD CON DETALLES Y ESTADO
    // =========================================================

    public async Task<PlanGuardiaEntity?> ObtenerEntidadPorIdAsync(
        int idPlanGuardia)
    {
        return await _context.PlanGuardia
            .Include(p => p.IdEstadoNavigation)
            .Include(p => p.DetallePlanGuardia)
            .FirstOrDefaultAsync(p =>
                p.IdPlanGuardia == idPlanGuardia);
    }

    // =========================================================
    // COMPROBAR SOLAPAMIENTO DE FECHAS
    // =========================================================

    public async Task<bool> ExisteSolapamientoAsync(
        DateTime fechaInicial,
        DateTime fechaFin,
        int? excluirIdPlanGuardia = null)
    {
        var query = _context.PlanGuardia
            .AsNoTracking()
            .Where(p =>
                p.FechaInicial <= fechaFin.Date &&
                p.FechaFin >= fechaInicial.Date);

        if (excluirIdPlanGuardia.HasValue)
        {
            query = query.Where(p =>
                p.IdPlanGuardia != excluirIdPlanGuardia.Value);
        }

        return await query.AnyAsync();
    }

    // =========================================================
    // COMPROBAR RESPONSABLE DE APROBACIÓN
    // =========================================================

    public async Task<bool> EsResponsableAprobacionAsync(
        int idPersona)
    {
        return await _context.ResponsableAprobacionGuardia
            .AsNoTracking()
            .AnyAsync(r =>
                r.IdPersona == idPersona &&
                r.Tipo == "APRUEBA");
    }

    // =========================================================
    // OBTENER O CREAR ESTADO
    // =========================================================

    public async Task<EstadoParte> ObtenerOCrearEstadoAsync(
        string codigo,
        string nombre,
        string descripcion,
        string color,
        string icono,
        int idPersona)
    {
        var estado = await _context.EstadoParte
            .FirstOrDefaultAsync(e => e.Codigo == codigo);

        if (estado is null)
        {
            estado = await _context.EstadoParte
                .FirstOrDefaultAsync(e => e.Nombre == nombre);
        }

        if (estado is null)
        {
            estado = new EstadoParte
            {
                Codigo = codigo,
                Nombre = nombre,
                Descripcion = descripcion,
                Color = color,
                Icono = icono,
                Activo = true,
                IdPersonaCrea = idPersona
            };

            _context.EstadoParte.Add(estado);
        }
        else
        {
            var codigoOcupado = await _context.EstadoParte
                .AnyAsync(e =>
                    e.Codigo == codigo &&
                    e.IdEstado != estado.IdEstado);

            if (!codigoOcupado)
            {
                estado.Codigo = codigo;
            }

            var nombreOcupado = await _context.EstadoParte
                .AnyAsync(e =>
                    e.Nombre == nombre &&
                    e.IdEstado != estado.IdEstado);

            if (!nombreOcupado)
            {
                estado.Nombre = nombre;
            }

            estado.Descripcion = descripcion;
            estado.Color = color;
            estado.Icono = icono;
            estado.Activo = true;
            estado.IdPersonaActualiza = idPersona;
        }

        await _context.SaveChangesAsync();

        return estado;
    }

    // =========================================================
    // CREAR PLAN CON SUS DETALLES
    // =========================================================

    public async Task CrearAsync(PlanGuardiaEntity plan)
    {
        _context.PlanGuardia.Add(plan);

        // EF Core guarda el encabezado y los detalles relacionados
        // en la misma operación de SaveChangesAsync.
        await _context.SaveChangesAsync();
    }

    // =========================================================
    // GUARDAR CAMBIOS
    // =========================================================

    public async Task GuardarCambiosAsync()
    {
        await _context.SaveChangesAsync();
    }

    // =========================================================
    // ELIMINAR PLAN
    // =========================================================

    public async Task EliminarAsync(PlanGuardiaEntity plan)
    {
        _context.PlanGuardia.Remove(plan);

        await _context.SaveChangesAsync();
    }

    // =========================================================
    // OBTENER INTEGRANTES DE GRUPOS DURANTE UN PERÍODO
    // =========================================================

    public async Task<IReadOnlyList<VistaGrupoGuardiaPersona>>
        ObtenerMiembrosGrupoPeriodoAsync(
            DateTime fechaInicial,
            DateTime fechaFin)
    {
        return await _context.VistaGrupoGuardiaPersona
            .AsNoTracking()
            .Where(m =>
                m.Activo == true &&
                m.FechaInicio <= fechaFin.Date &&
                (
                    m.FechaFin == null ||
                    m.FechaFin >= fechaInicial.Date
                ))
            .OrderBy(m => m.IdGrupoGuardia)
            .ThenBy(m => m.IdPersona)
            .ToListAsync();
    }

    // =========================================================
    // OBTENER AFECTACIONES DE PERSONAS DURANTE UN PERÍODO
    // =========================================================

    public async Task<IReadOnlyList<VistaPersonasAfectaciones>>
        ObtenerAfectacionesPeriodoAsync(
            IReadOnlyCollection<int> idsPersonas,
            DateTime fechaInicial,
            DateTime fechaFin)
    {
        if (idsPersonas.Count == 0)
        {
            return Array.Empty<VistaPersonasAfectaciones>();
        }

        return await _context.VistaPersonasAfectaciones
            .AsNoTracking()
            .Where(a =>
                idsPersonas.Contains(a.IdPersona) &&
                a.IdAfectacion.HasValue &&
                a.FechaHoraInicio.HasValue &&
                a.FechaHoraInicio.Value < fechaFin.Date.AddDays(1) &&
                (
                    !a.FechaHoraFin.HasValue ||
                    a.FechaHoraFin.Value >= fechaInicial.Date
                ))
            .ToListAsync();
    }

    // =========================================================
    // OBTENER HISTORIAL DE GUARDIAS APROBADAS
    // =========================================================

    public async Task<IReadOnlyList<RegistroGuardiaHistoricaDto>>
        ObtenerHistorialAprobadoAsync(
            DateTime fechaInicial,
            DateTime fechaFin)
    {
        var query =
            from d in _context.DetallePlanGuardia.AsNoTracking()
            join p in _context.PlanGuardia.AsNoTracking()
                on d.IdPlanGuardia equals p.IdPlanGuardia
            join e in _context.EstadoParte.AsNoTracking()
                on p.IdEstado equals e.IdEstado
            where d.FechaGuardia >= fechaInicial.Date
                  && d.FechaGuardia <= fechaFin.Date
                  && (
                      e.Codigo == "APROBADO" ||
                      e.Nombre == "Aprobado"
                  )
            select new RegistroGuardiaHistoricaDto
            {
                IdPersona = d.IdPersona,
                FechaGuardia = d.FechaGuardia
            };

        return await query.ToListAsync();
    }


    // =========================================================
// OBTENER FERIADOS ACTIVOS DE UN PERÍODO
// =========================================================

public async Task<IReadOnlyList<CalendarioFeriado>> ObtenerFeriadosPeriodoAsync(
        DateTime fechaInicial,
        DateTime fechaFin)
{
    return await _context.CalendarioFeriado
        .AsNoTracking()
        .Where(f =>
            f.Activo &&
            f.Fecha >= fechaInicial.Date &&
            f.Fecha <= fechaFin.Date)
        .OrderBy(f => f.Fecha)
        .ThenBy(f => f.Nombre)
        .ToListAsync();
}
}