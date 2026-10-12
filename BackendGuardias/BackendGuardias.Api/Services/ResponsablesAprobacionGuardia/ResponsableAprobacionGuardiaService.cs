using BackendGuardias.Api.DTOs.Comun;
using BackendGuardias.Api.DTOs.ResponsablesAprobacionGuardia;
using BackendGuardias.Api.Models.Entities;
using BackendGuardias.Api.Models.Views;
using BackendGuardias.Api.Repositories.ResponsablesAprobacionGuardia;

namespace BackendGuardias.Api.Services.ResponsablesAprobacionGuardia;

public sealed class ResponsableAprobacionGuardiaService
    : IResponsableAprobacionGuardiaService
{
    private readonly IResponsableAprobacionGuardiaRepository _repository;

    public ResponsableAprobacionGuardiaService(
        IResponsableAprobacionGuardiaRepository repository)
    {
        _repository = repository;
    }

    public async Task<ResultadoConsultaDto<ResponsableAprobacionGuardiaDto>>
        ObtenerTodosAsync(
            string tipo,
            int? idUnidadOrganizativa,
            int? idPersona,
            string? buscar,
            CancellationToken cancellationToken = default)
    {
        ValidarTipo(tipo);

        if (idUnidadOrganizativa is <= 0)
        {
            throw new ArgumentException(
                "El ID de unidad organizativa debe ser mayor que cero.");
        }

        if (idPersona is <= 0)
        {
            throw new ArgumentException(
                "El ID de persona debe ser mayor que cero.");
        }

        var registros = await _repository.ObtenerTodosAsync(
            tipo,
            idUnidadOrganizativa,
            idPersona,
            buscar,
            cancellationToken);

        var data = registros.Select(Mapear).ToList();

        return new ResultadoConsultaDto<ResponsableAprobacionGuardiaDto>
        {
            Total = data.Count,
            Data = data
        };
    }

    public async Task<ResponsableAprobacionGuardiaDto?> ObtenerPorIdAsync(
        int idResponsable,
        string tipo,
        CancellationToken cancellationToken = default)
    {
        ValidarId(idResponsable);
        ValidarTipo(tipo);

        var registro = await _repository.ObtenerVistaPorIdAsync(
            idResponsable,
            tipo,
            cancellationToken);

        return registro is null ? null : Mapear(registro);
    }

    public async Task<ResponsableAprobacionGuardiaDto> CrearAsync(
        CrearResponsableAprobacionGuardiaDto dto,
        string tipo,
        int idPersonaOperador,
        CancellationToken cancellationToken = default)
    {
        ValidarTipo(tipo);
        ValidarIds(dto.IdPersona, dto.IdUnidadOrganizativa);

        if (idPersonaOperador <= 0)
        {
            throw new ArgumentException(
                "El ID del operador debe ser mayor que cero.");
        }

        if (!await _repository.ExistePersonaAsync(
                dto.IdPersona,
                cancellationToken))
        {
            throw new ArgumentException(
                "La persona seleccionada no existe o no está disponible.");
        }

        if (!await _repository.ExistePersonaAsync(
                idPersonaOperador,
                cancellationToken))
        {
            throw new ArgumentException(
                "La persona que realiza la operación no existe o no está disponible.");
        }

        if (!await _repository.ExisteUnidadOrganizativaAsync(
                dto.IdUnidadOrganizativa,
                cancellationToken))
        {
            throw new ArgumentException(
                "La unidad organizativa indicada no existe.");
        }

        var duplicado = await _repository.ExisteTipoEnUnidadAsync(
            dto.IdUnidadOrganizativa,
            tipo,
            cancellationToken: cancellationToken);

        if (duplicado)
        {
            throw new InvalidOperationException(
                $"La unidad ya tiene un responsable de tipo {tipo}.");
        }

        var ahora = DateTime.Now;

        var responsable = new ResponsableAprobacionGuardia
        {
            IdPersona = dto.IdPersona,
            IdUnidadOrganizativa = dto.IdUnidadOrganizativa,
            Tipo = tipo,
            IdPersonaCrea = idPersonaOperador,
            FechaHoraCreado = ahora,
            FechaHoraActualizado = ahora
        };

        await _repository.AgregarAsync(
            responsable,
            cancellationToken);

        await _repository.GuardarCambiosAsync(
            cancellationToken);

        var resultado = await _repository.ObtenerVistaPorIdAsync(
            responsable.IdResponsable,
            tipo,
            cancellationToken);

        if (resultado is null)
        {
            throw new InvalidOperationException(
                "El responsable fue guardado, pero no se pudo recuperar desde la vista.");
        }

        return Mapear(resultado);
    }

    public async Task<ResponsableAprobacionGuardiaDto?> ActualizarAsync(
        int idResponsable,
        ActualizarResponsableAprobacionGuardiaDto dto,
        string tipo,
        CancellationToken cancellationToken = default)
    {
        ValidarId(idResponsable);
        ValidarTipo(tipo);
        ValidarIds(dto.IdPersona, dto.IdUnidadOrganizativa);

        var responsable = await _repository.ObtenerEntidadPorIdAsync(
            idResponsable,
            tipo,
            cancellationToken);

        if (responsable is null)
        {
            return null;
        }

        if (!await _repository.ExistePersonaAsync(
                dto.IdPersona,
                cancellationToken))
        {
            throw new ArgumentException(
                "La persona seleccionada no existe o no está disponible.");
        }

        if (!await _repository.ExisteUnidadOrganizativaAsync(
                dto.IdUnidadOrganizativa,
                cancellationToken))
        {
            throw new ArgumentException(
                "La unidad organizativa indicada no existe.");
        }

        var duplicado = await _repository.ExisteTipoEnUnidadAsync(
            dto.IdUnidadOrganizativa,
            tipo,
            idResponsable,
            cancellationToken);

        if (duplicado)
        {
            throw new InvalidOperationException(
                $"La unidad ya tiene otro responsable de tipo {tipo}.");
        }

        responsable.IdPersona = dto.IdPersona;
        responsable.IdUnidadOrganizativa = dto.IdUnidadOrganizativa;

        // Se conserva el creador original.
        // La entidad actual no contiene IdPersonaActualiza.
        responsable.FechaHoraActualizado = DateTime.Now;

        await _repository.GuardarCambiosAsync(
            cancellationToken);

        var resultado = await _repository.ObtenerVistaPorIdAsync(
            idResponsable,
            tipo,
            cancellationToken);

        return resultado is null ? null : Mapear(resultado);
    }

    public async Task<bool> EliminarAsync(
        int idResponsable,
        string tipo,
        CancellationToken cancellationToken = default)
    {
        ValidarId(idResponsable);
        ValidarTipo(tipo);

        var responsable = await _repository.ObtenerEntidadPorIdAsync(
            idResponsable,
            tipo,
            cancellationToken);

        if (responsable is null)
        {
            return false;
        }

        _repository.Eliminar(responsable);

        await _repository.GuardarCambiosAsync(
            cancellationToken);

        return true;
    }

    private static ResponsableAprobacionGuardiaDto Mapear(
        VistaResponsableAprobacionGuardia registro)
    {
        return new ResponsableAprobacionGuardiaDto
        {
            IdResponsable = registro.IdResponsable,
            IdUnidadOrganizativa = registro.IdUnidadOrganizativa,
            UnidadOrganizativaNombre = registro.UnidadOrganizativaNombre,
            UnidadOrganizativaDescripcion =
                registro.UnidadOrganizativaDescripcion,
            IdCategoriaUnidadOrganizativa = registro.IdCategoriaUnidadOrganizativa,
            CategoriaUnidadOrganizativaNombre = registro.CategoriaUnidadOrganizativaNombre,
            CategoriaUnidadOrganizativaDescripcion = registro.CategoriaUnidadOrganizativaDescripcion,
            IdPersona = registro.IdPersona,
            Nombre = registro.Nombre,
            Apellidos = registro.Apellidos,
            PersonaNombreCompleto = registro.PersonaNombreCompleto,
            CarnetIdentidad = registro.CarnetIdentidad,
            IdSexo = registro.IdSexo,
            SexoNombre = registro.SexoNombre,
            SexoDescripcion = registro.SexoDescripcion,
            Sexo = registro.Sexo,
            IdColorPiel = registro.IdColorPiel,
            ColorPielNombre = registro.ColorPielNombre,
            ColorPielDescripcion = registro.ColorPielDescripcion,
            IdCargoUnidadOrganizativa = registro.IdCargoUnidadOrganizativa,
            CargoNombre = registro.CargoNombre,
            IdUnidadOrganizativaPersona = registro.IdUnidadOrganizativaPersona,
            UnidadOrganizativaPersonaNombre = registro.UnidadOrganizativaPersonaNombre,
            UnidadOrganizativaPersonaDescripcion = registro.UnidadOrganizativaPersonaDescripcion,
            IdCargo = registro.IdCargo,
            CargoDescripcion = registro.CargoDescripcion,
            IdGrado = registro.IdGrado,
            GradoDescripcion = registro.GradoDescripcion,
            GradoNombre = registro.GradoNombre,
            Grado = registro.Grado,
            Tipo = registro.Tipo,
            IdPersonaCrea = registro.IdPersonaCrea,
            PersonaCreaNombre = registro.PersonaCreaNombre,
            FechaHoraCreado = registro.FechaHoraCreado,
            FechaHoraActualizado = registro.FechaHoraActualizado
        };
    }

    private static void ValidarTipo(string tipo)
    {
        if (tipo != TipoResponsableAprobacionGuardia.Planificador
            && tipo != TipoResponsableAprobacionGuardia.Aprueba)
        {
            throw new ArgumentException(
                "El tipo debe ser PLANIFICADOR o APRUEBA.");
        }
    }

    private static void ValidarId(int idResponsable)
    {
        if (idResponsable <= 0)
        {
            throw new ArgumentException(
                "El ID del responsable debe ser mayor que cero.");
        }
    }

    private static void ValidarIds(
        int idPersona,
        int idUnidadOrganizativa)
    {
        if (idPersona <= 0)
        {
            throw new ArgumentException(
                "El ID de persona debe ser mayor que cero.");
        }

        if (idUnidadOrganizativa <= 0)
        {
            throw new ArgumentException(
                "El ID de unidad organizativa debe ser mayor que cero.");
        }
    }
}