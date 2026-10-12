using BackendGuardias.Api.DTOs.Comun;
using BackendGuardias.Api.DTOs.EstadosParte;
using BackendGuardias.Api.Models.Entities;
using BackendGuardias.Api.Repositories.EstadosParte;

namespace BackendGuardias.Api.Services.EstadosParte;

public sealed class EstadoParteService : IEstadoParteService
{
    private readonly IEstadoParteRepository _repository;

    public EstadoParteService(IEstadoParteRepository repository)
    {
        _repository = repository;
    }

    public async Task<ResultadoConsultaDto<EstadoParteDto>>
        ObtenerTodosAsync(
            string? buscar,
            bool? activo,
            CancellationToken cancellationToken = default)
    {
        var estados = await _repository.ObtenerTodosAsync(
            buscar,
            activo,
            cancellationToken);

        var data = estados.Select(Mapear).ToList();

        return new ResultadoConsultaDto<EstadoParteDto>
        {
            Total = data.Count,
            Data = data
        };
    }

    public async Task<EstadoParteDto?> ObtenerPorIdAsync(
        int idEstado,
        CancellationToken cancellationToken = default)
    {
        ValidarIdEstado(idEstado);

        var estado = await _repository.ObtenerPorIdAsync(
            idEstado,
            cancellationToken);

        return estado is null ? null : Mapear(estado);
    }

    public async Task<EstadoParteDto> CrearAsync(
        CrearEstadoParteDto dto,
        int idPersonaOperador,
        CancellationToken cancellationToken = default)
    {
        ValidarOperador(idPersonaOperador);

        var codigo = ValidarTexto(
            dto.Codigo,
            nameof(dto.Codigo),
            "El código es obligatorio.");

        var nombre = ValidarTexto(
            dto.Nombre,
            nameof(dto.Nombre),
            "El nombre es obligatorio.");

        if (await _repository.ExisteCodigoAsync(
                codigo,
                cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException(
                $"Ya existe un estado con el código '{codigo}'.");
        }

        var ahora = DateTime.Now;

        var estado = new EstadoParte
        {
            Codigo = codigo,
            Nombre = nombre,
            Descripcion = NormalizarOpcional(dto.Descripcion),
            Color = NormalizarOpcional(dto.Color),
            Icono = NormalizarOpcional(dto.Icono),
            Activo = dto.Activo ?? true,

            IdPersonaCrea = idPersonaOperador,
            IdPersonaActualiza = null,
            FechaHoraCreado = ahora,
            FechaHoraActualizado = ahora
        };

        await _repository.AgregarAsync(
            estado,
            cancellationToken);

        await _repository.GuardarCambiosAsync(
            cancellationToken);

        return Mapear(estado);
    }

    public async Task<EstadoParteDto?> ActualizarAsync(
        int idEstado,
        ActualizarEstadoParteDto dto,
        int idPersonaOperador,
        CancellationToken cancellationToken = default)
    {
        ValidarIdEstado(idEstado);
        ValidarOperador(idPersonaOperador);

        var estado = await _repository.ObtenerPorIdAsync(
            idEstado,
            cancellationToken);

        if (estado is null)
        {
            return null;
        }

        var codigo = ValidarTexto(
            dto.Codigo,
            nameof(dto.Codigo),
            "El código es obligatorio.");

        var nombre = ValidarTexto(
            dto.Nombre,
            nameof(dto.Nombre),
            "El nombre es obligatorio.");

        if (await _repository.ExisteCodigoAsync(
                codigo,
                idEstado,
                cancellationToken))
        {
            throw new InvalidOperationException(
                $"Ya existe otro estado con el código '{codigo}'.");
        }

        estado.Codigo = codigo;
        estado.Nombre = nombre;
        estado.Descripcion = NormalizarOpcional(dto.Descripcion);
        estado.Color = NormalizarOpcional(dto.Color);
        estado.Icono = NormalizarOpcional(dto.Icono);

        // Si no se envía Activo, se conserva el valor existente.
        estado.Activo = dto.Activo ?? estado.Activo ?? true;

        // Auditoría de modificación.
        estado.IdPersonaActualiza = idPersonaOperador;
        estado.FechaHoraActualizado = DateTime.Now;

        await _repository.GuardarCambiosAsync(
            cancellationToken);

        return Mapear(estado);
    }

    public async Task<bool> DesactivarAsync(
        int idEstado,
        int idPersonaOperador,
        CancellationToken cancellationToken = default)
    {
        ValidarIdEstado(idEstado);
        ValidarOperador(idPersonaOperador);

        var estado = await _repository.ObtenerPorIdAsync(
            idEstado,
            cancellationToken);

        if (estado is null)
        {
            return false;
        }

        // Baja lógica: no se elimina el registro físico.
        estado.Activo = false;

        estado.IdPersonaActualiza = idPersonaOperador;
        estado.FechaHoraActualizado = DateTime.Now;

        await _repository.GuardarCambiosAsync(
            cancellationToken);

        return true;
    }

    private static EstadoParteDto Mapear(EstadoParte estado)
    {
        return new EstadoParteDto
        {
            IdEstado = estado.IdEstado,
            Codigo = estado.Codigo,
            Nombre = estado.Nombre,
            Descripcion = estado.Descripcion,
            Color = estado.Color,
            Icono = estado.Icono,
            Activo = estado.Activo,
            IdPersonaCrea = estado.IdPersonaCrea,
            IdPersonaActualiza = estado.IdPersonaActualiza,
            FechaHoraCreado = estado.FechaHoraCreado,
            FechaHoraActualizado = estado.FechaHoraActualizado
        };
    }

    private static string ValidarTexto(
        string? valor,
        string nombrePropiedad,
        string mensaje)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ArgumentException(mensaje, nombrePropiedad);
        }

        return valor.Trim();
    }

    private static string? NormalizarOpcional(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor)
            ? null
            : valor.Trim();
    }

    private static void ValidarIdEstado(int idEstado)
    {
        if (idEstado <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(idEstado),
                "El ID del estado debe ser mayor que cero.");
        }
    }

    private static void ValidarOperador(int idPersonaOperador)
    {
        if (idPersonaOperador <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(idPersonaOperador),
                "El ID de la persona que realiza la operación debe ser mayor que cero.");
        }
    }
}