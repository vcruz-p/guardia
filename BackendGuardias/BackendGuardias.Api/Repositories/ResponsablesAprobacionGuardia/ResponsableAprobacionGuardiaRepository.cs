using BackendGuardias.Api.Data;
using BackendGuardias.Api.Models.Entities;
using BackendGuardias.Api.Models.Views;
using Microsoft.EntityFrameworkCore;

namespace BackendGuardias.Api.Repositories.ResponsablesAprobacionGuardia;

public sealed class ResponsableAprobacionGuardiaRepository
    : IResponsableAprobacionGuardiaRepository
{
    private readonly GuardiasDbContext _context;

    public ResponsableAprobacionGuardiaRepository(
        GuardiasDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<VistaResponsableAprobacionGuardia>>
        ObtenerTodosAsync(
            string tipo,
            int? idUnidadOrganizativa,
            int? idPersona,
            string? buscar,
            CancellationToken cancellationToken = default)
    {
        var query = _context.VistaResponsableAprobacionGuardia
            .AsNoTracking()
            .Where(x => x.Tipo == tipo);

        if (idUnidadOrganizativa.HasValue)
        {
            query = query.Where(
                x => x.IdUnidadOrganizativa == idUnidadOrganizativa.Value);
        }

        if (idPersona.HasValue)
        {
            query = query.Where(
                x => x.IdPersona == idPersona.Value);
        }

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var filtro = buscar.Trim();

            query = query.Where(x =>
                x.UnidadOrganizativaNombre.Contains(filtro)
                || x.Nombre.Contains(filtro)
                || (x.Apellidos != null
                    && x.Apellidos.Contains(filtro))
                || (x.PersonaNombreCompleto != null
                    && x.PersonaNombreCompleto.Contains(filtro))
                || (x.CarnetIdentidad != null
                    && x.CarnetIdentidad.Contains(filtro)));
        }

        return await query
            .OrderBy(x => x.UnidadOrganizativaNombre)
            .ThenBy(x => x.PersonaNombreCompleto)
            .ToListAsync(cancellationToken);
    }

    public Task<VistaResponsableAprobacionGuardia?>
        ObtenerVistaPorIdAsync(
            int idResponsable,
            string tipo,
            CancellationToken cancellationToken = default)
    {
        return _context.VistaResponsableAprobacionGuardia
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.IdResponsable == idResponsable
                     && x.Tipo == tipo,
                cancellationToken);
    }

    public Task<ResponsableAprobacionGuardia?>
        ObtenerEntidadPorIdAsync(
            int idResponsable,
            string tipo,
            CancellationToken cancellationToken = default)
    {
        return _context.ResponsableAprobacionGuardia
            .FirstOrDefaultAsync(
                x => x.IdResponsable == idResponsable
                     && x.Tipo == tipo,
                cancellationToken);
    }

    public async Task<bool> ExistePersonaAsync(
        int idPersona,
        CancellationToken cancellationToken = default)
    {
        var total = await _context.Database
            .SqlQuery<long>($"""
                SELECT COUNT(*) AS Value
                FROM guardia.vista_personas_unidades_subordinacion
                WHERE id_persona = {idPersona}
                """)
            .SingleAsync(cancellationToken);

        return total > 0;
    }

    public async Task<bool> ExisteUnidadOrganizativaAsync(
        int idUnidadOrganizativa,
        CancellationToken cancellationToken = default)
    {
        var total = await _context.Database
            .SqlQuery<long>($"""
                SELECT COUNT(*) AS Value
                FROM estructura.unidad_organizativa
                WHERE id = {idUnidadOrganizativa}
                """)
            .SingleAsync(cancellationToken);

        return total > 0;
    }

    public Task<bool> ExisteTipoEnUnidadAsync(
        int idUnidadOrganizativa,
        string tipo,
        int? idResponsableExcluir = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ResponsableAprobacionGuardia
            .AsNoTracking()
            .Where(x =>
                x.IdUnidadOrganizativa == idUnidadOrganizativa
                && x.Tipo == tipo);

        if (idResponsableExcluir.HasValue)
        {
            query = query.Where(
                x => x.IdResponsable != idResponsableExcluir.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public async Task AgregarAsync(
        ResponsableAprobacionGuardia responsable,
        CancellationToken cancellationToken = default)
    {
        await _context.ResponsableAprobacionGuardia
            .AddAsync(responsable, cancellationToken);
    }

    public void Eliminar(ResponsableAprobacionGuardia responsable)
    {
        _context.ResponsableAprobacionGuardia.Remove(responsable);
    }

    public Task<int> GuardarCambiosAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}