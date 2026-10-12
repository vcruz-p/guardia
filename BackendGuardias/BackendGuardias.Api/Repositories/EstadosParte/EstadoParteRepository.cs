using BackendGuardias.Api.Data;
using BackendGuardias.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackendGuardias.Api.Repositories.EstadosParte;

public sealed class EstadoParteRepository : IEstadoParteRepository
{
    private readonly GuardiasDbContext _context;

    public EstadoParteRepository(GuardiasDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<EstadoParte>> ObtenerTodosAsync(
        string? buscar,
        bool? activo,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<EstadoParte>()
            .AsNoTracking()
            .AsQueryable();

        if (activo.HasValue)
        {
            query = query.Where(x => x.Activo == activo.Value);
        }

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var filtro = buscar.Trim();

            query = query.Where(x =>
                x.Codigo.Contains(filtro)
                || x.Nombre.Contains(filtro)
                || (
                    x.Descripcion != null
                    && x.Descripcion.Contains(filtro)
                ));
        }

        return await query
            .OrderBy(x => x.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<EstadoParte?> ObtenerPorIdAsync(
        int idEstado,
        CancellationToken cancellationToken = default)
    {
        // Se deja tracked para permitir modificaciones.
        return await _context.Set<EstadoParte>()
            .FirstOrDefaultAsync(
                x => x.IdEstado == idEstado,
                cancellationToken);
    }

    public async Task<bool> ExisteCodigoAsync(
        string codigo,
        int? idEstadoExcluir = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<EstadoParte>()
            .AsNoTracking()
            .Where(x => x.Codigo == codigo);

        if (idEstadoExcluir.HasValue)
        {
            query = query.Where(
                x => x.IdEstado != idEstadoExcluir.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AgregarAsync(
        EstadoParte estado,
        CancellationToken cancellationToken = default)
    {
        await _context.Set<EstadoParte>()
            .AddAsync(estado, cancellationToken);
    }

    public Task<int> GuardarCambiosAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}