using BackendGuardias.Api.DTOs.GruposGuardia;
using BackendGuardias.Api.Models.Entities;
using BackendGuardias.Api.Repositories.GruposGuardia;

namespace BackendGuardias.Api.Services.GruposGuardia;

public sealed class GrupoGuardiaService : IGrupoGuardiaService
{
    private readonly IGrupoGuardiaRepository _repository;

    public GrupoGuardiaService(IGrupoGuardiaRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<GrupoGuardiaDto>> ObtenerTodosAsync(
        bool incluirInactivos,
        CancellationToken cancellationToken)
    {
        var grupos = await _repository.ObtenerTodosAsync(
            incluirInactivos,
            cancellationToken);

        return grupos.Select(Mapear).ToArray();
    }

    public async Task<GrupoGuardiaDto?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var grupo = await _repository.ObtenerPorIdAsync(id, cancellationToken);
        return grupo is null ? null : Mapear(grupo);
    }

    public async Task<GrupoGuardiaDto> CrearAsync(
        CrearGrupoGuardiaDto dto,
        int? idPersonaActual,
        CancellationToken cancellationToken)
    {
        var nombre = dto.Nombre.Trim();
        var iniciales = dto.Iniciales.Trim();

        if (await _repository.ExisteNombreOInicialesAsync(
                nombre,
                iniciales,
                idExcluir: null,
                cancellationToken))
        {
            throw new GrupoGuardiaDuplicadoException();
        }

        var fechaHoraActual = DateTime.Now;
        var entidad = new GrupoGuardia
        {
            Nombre = nombre,
            Iniciales = iniciales,
            SubordinadoAIdGrupoGuardia =
                dto.SubordinadoAIdGrupoGuardia,
            Color = dto.Color,
            Icono = dto.Icono,
            Observacion = dto.Observacion,
            Activo = true,
            IdPersonaCrea = idPersonaActual,
            FechaHoraCreado = fechaHoraActual,
            FechaHoraActualizado = fechaHoraActual
        };

        _repository.Agregar(entidad);
        await _repository.GuardarCambiosAsync(cancellationToken);
        return Mapear(entidad);
    }

    public async Task<GrupoGuardiaDto?> ActualizarAsync(
        int id,
        ActualizarGrupoGuardiaDto dto,
        int? idPersonaActual,
        CancellationToken cancellationToken)
    {
        var entidad = await _repository.ObtenerParaActualizarAsync(
            id,
            cancellationToken);

        if (entidad is null)
        {
            return null;
        }

        var nombre = dto.Nombre.Trim();
        var iniciales = dto.Iniciales.Trim();

        if (await _repository.ExisteNombreOInicialesAsync(
                nombre,
                iniciales,
                id,
                cancellationToken))
        {
            throw new GrupoGuardiaDuplicadoException();
        }

        entidad.Nombre = nombre;
        entidad.Iniciales = iniciales;
        entidad.SubordinadoAIdGrupoGuardia =
            dto.SubordinadoAIdGrupoGuardia;

        entidad.Color =
            dto.Color;

        entidad.Icono =
            dto.Icono;

        entidad.Observacion =
            dto.Observacion;

        entidad.Activo =
            dto.Activo;

        entidad.IdPersonaActualiza =
            idPersonaActual;

        entidad.FechaHoraActualizado =
            DateTime.Now;

        await _repository.GuardarCambiosAsync(cancellationToken);
        return Mapear(entidad);
    }

    public async Task<bool> CambiarEstadoAsync(
        int id,
        bool activo,
        int? idPersonaActual,
        CancellationToken cancellationToken)
    {
        var entidad = await _repository.ObtenerParaActualizarAsync(
            id,
            cancellationToken);

        if (entidad is null)
        {
            return false;
        }

        entidad.Activo = activo;
        entidad.IdPersonaActualiza = idPersonaActual;
        entidad.FechaHoraActualizado = DateTime.Now;

        await _repository.GuardarCambiosAsync(cancellationToken);

        return true;
    }

    private static GrupoGuardiaDto Mapear(GrupoGuardia entidad)
    {
        return new GrupoGuardiaDto
        {
            IdGrupoGuardia = entidad.IdGrupoGuardia,
            Nombre = entidad.Nombre,
            Iniciales = entidad.Iniciales,
            SubordinadoAIdGrupoGuardia = entidad.SubordinadoAIdGrupoGuardia,
            Color = entidad.Color,
            Icono = entidad.Icono,
            Observacion = entidad.Observacion,
            Activo = entidad.Activo ?? false,
            IdPersonaCrea = entidad.IdPersonaCrea,
            IdPersonaActualiza = entidad.IdPersonaActualiza,
            FechaHoraCreado = entidad.FechaHoraCreado,
            FechaHoraActualizado = entidad.FechaHoraActualizado
        };
    }
}