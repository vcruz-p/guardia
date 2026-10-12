using BackendGuardias.Api.Data;
using BackendGuardias.Api.Models.Entities;
using BackendGuardias.Api.Models.Views;
using Microsoft.EntityFrameworkCore;

namespace BackendGuardias.Api.Repositories.GruposGuardia;

public sealed class GrupoGuardiaRepository : IGrupoGuardiaRepository
{
    private readonly GuardiasDbContext _context;

    public GrupoGuardiaRepository(GuardiasDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<VistaGruposGuardia>> ObtenerVistasAsync(
        bool incluirInactivos,
        CancellationToken cancellationToken)
    {
        var query = _context.VistaGruposGuardia.AsNoTracking();

        if (!incluirInactivos)
        {
            query = query.Where(grupo => grupo.Activo == true);
        }

        return await query
            .OrderBy(grupo => grupo.GrupoNombre)
            .ToListAsync(cancellationToken);
    }

    public Task<VistaGruposGuardia?> ObtenerVistaPorIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return _context.VistaGruposGuardia
            .AsNoTracking()
            .FirstOrDefaultAsync(grupo => grupo.IdGrupoGuardia == id, cancellationToken);
    }

    public async Task<IReadOnlyList<GrupoGuardia>> ObtenerTodosAsync(
        bool incluirInactivos,
        CancellationToken cancellationToken)
    {
        var query = _context.GrupoGuardia.AsNoTracking();

        if (!incluirInactivos)
        {
            query = query.Where(grupo => grupo.Activo == true);
        }

        return await query
            .OrderBy(grupo => grupo.Nombre)
            .ToListAsync(cancellationToken);
    }

    public Task<GrupoGuardia?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return _context.GrupoGuardia
            .AsNoTracking()
            .FirstOrDefaultAsync(
                grupo => grupo.IdGrupoGuardia == id,
                cancellationToken);
    }

    public Task<GrupoGuardia?> ObtenerParaActualizarAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return _context.GrupoGuardia
            .FirstOrDefaultAsync(
                grupo => grupo.IdGrupoGuardia == id,
                cancellationToken);
    }

    public Task<bool> ExisteNombreOInicialesAsync(
        string nombre,
        string iniciales,
        int? idExcluir,
        CancellationToken cancellationToken)
    {
        return _context.GrupoGuardia.AnyAsync(
            grupo =>
                (grupo.Nombre == nombre || grupo.Iniciales == iniciales) &&
                (!idExcluir.HasValue || grupo.IdGrupoGuardia != idExcluir.Value),
            cancellationToken);
    }

    public void Agregar(GrupoGuardia grupo)
    {
        _context.GrupoGuardia.Add(grupo);
    }

    public async Task GuardarCambiosAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
