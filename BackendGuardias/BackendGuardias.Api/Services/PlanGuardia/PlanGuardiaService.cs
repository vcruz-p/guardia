using BackendGuardias.Api.DTOs.PlanGuardia;
using BackendGuardias.Api.Models.Entities;
using BackendGuardias.Api.Models.Views;
using BackendGuardias.Api.Repositories.PlanGuardia;
using Microsoft.Extensions.Logging;

using PlanGuardiaEntity =
    BackendGuardias.Api.Models.Entities.PlanGuardia;

namespace BackendGuardias.Api.Services.PlanGuardia;

public sealed class PlanGuardiaService : IPlanGuardiaService
{
    private const string EstadoBorradorCodigo = "BORRADOR";
    private const string EstadoAprobadoCodigo = "APROBADO";

    private readonly IPlanGuardiaRepository _repository;
    private readonly ILogger<PlanGuardiaService> _logger;

    public PlanGuardiaService(
        IPlanGuardiaRepository repository,
        ILogger<PlanGuardiaService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    // =========================================================
    // CONSULTAR PLANES
    // =========================================================

    public async Task<IReadOnlyList<VistaPlanGuardia>> ObtenerTodosAsync(
        int? idEstado = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null)
    {
        if (fechaDesde.HasValue &&
            fechaHasta.HasValue &&
            fechaDesde.Value.Date > fechaHasta.Value.Date)
        {
            throw new ArgumentException(
                "La fecha inicial del filtro no puede superar la fecha final.");
        }

        return await _repository.ObtenerTodosAsync(
            idEstado,
            fechaDesde,
            fechaHasta);
    }

    public async Task<VistaPlanGuardia> ObtenerPorIdAsync(
        int idPlanGuardia)
    {
        ValidarId(idPlanGuardia, nameof(idPlanGuardia));

        return await _repository.ObtenerVistaPorIdAsync(idPlanGuardia)
            ?? throw new KeyNotFoundException(
                $"No existe el plan de guardia con ID {idPlanGuardia}.");
    }

    public async Task<IReadOnlyList<VistaDetallePlanGuardia>> ObtenerDetallesAsync(
        int idPlanGuardia)
    {
        ValidarId(idPlanGuardia, nameof(idPlanGuardia));

        // Validar que exista el encabezado: una lista vacía de detalles
        // no debe confundirse con un plan inexistente.
        _ = await ObtenerPorIdAsync(idPlanGuardia);
        return await _repository.ObtenerDetallesAsync(idPlanGuardia);
    }

    // =========================================================
    // CREAR PLAN CON SUS DETALLES
    // =========================================================

    public async Task<VistaPlanGuardia> CrearAsync(
        PlanGuardiaCrearDto dto,
        int idPersonaActor)
    {
        ArgumentNullException.ThrowIfNull(dto);

        ValidarId(idPersonaActor, nameof(idPersonaActor));

        ValidarPeriodo(
            dto.FechaInicial,
            dto.FechaFin);

        ValidarDetalles(
            dto.Detalles,
            dto.FechaInicial,
            dto.FechaFin);

        await ValidarSolapamientoAsync(
            dto.FechaInicial,
            dto.FechaFin);

        var borrador = await ObtenerEstadoBorradorAsync(
            idPersonaActor);

        var ahora = DateTime.Now;

        var plan = new PlanGuardiaEntity
        {
            FechaInicial = dto.FechaInicial.Date,
            FechaFin = dto.FechaFin.Date,
            IdEstado = borrador.IdEstado,
            IdEstadoNavigation = borrador,
            IdPersonaCrea = idPersonaActor,
            Observacion = NormalizarTexto(dto.Observacion, 1000),
            FechaHoraCreado = ahora,
            FechaHoraActualizado = ahora
        };

        foreach (var item in dto.Detalles)
        {
            plan.DetallePlanGuardia.Add(
                CrearDetalle(
                    item,
                    idPersonaActor,
                    plan,
                    ahora));
        }

        // El encabezado y los detalles se guardan en una operación.
        await _repository.CrearAsync(plan);

        return await ObtenerPorIdAsync(plan.IdPlanGuardia);
    }

    // =========================================================
    // EDITAR PLAN Y REEMPLAZAR DETALLES
    // =========================================================

    public async Task<VistaPlanGuardia> ActualizarAsync(
        int idPlanGuardia,
        PlanGuardiaActualizarDto dto,
        int idPersonaActor)
    {
        ArgumentNullException.ThrowIfNull(dto);

        ValidarId(idPlanGuardia, nameof(idPlanGuardia));
        ValidarId(idPersonaActor, nameof(idPersonaActor));

        ValidarPeriodo(
            dto.FechaInicial,
            dto.FechaFin);

        ValidarDetalles(
            dto.Detalles,
            dto.FechaInicial,
            dto.FechaFin);

        var plan = await ObtenerEntidadExistenteAsync(idPlanGuardia);

        if (EsEstadoAprobado(plan.IdEstadoNavigation))
        {
            throw new InvalidOperationException(
                "No se puede editar un plan aprobado.");
        }

        await ValidarSolapamientoAsync(
            dto.FechaInicial,
            dto.FechaFin,
            idPlanGuardia);

        plan.FechaInicial = dto.FechaInicial.Date;
        plan.FechaFin = dto.FechaFin.Date;
        plan.Observacion = NormalizarTexto(dto.Observacion, 1000);
        plan.IdPersonaActualiza = idPersonaActor;
        plan.FechaHoraActualizado = DateTime.Now;

        // Reemplazar las asignaciones del plan pendiente.
        plan.DetallePlanGuardia.Clear();

        var ahora = DateTime.Now;

        foreach (var item in dto.Detalles)
        {
            plan.DetallePlanGuardia.Add(
                CrearDetalle(
                    item,
                    idPersonaActor,
                    plan,
                    ahora));
        }

        await _repository.GuardarCambiosAsync();

        return await ObtenerPorIdAsync(plan.IdPlanGuardia);
    }

    // =========================================================
    // ELIMINAR PLAN NO APROBADO
    // =========================================================

    public async Task EliminarAsync(int idPlanGuardia)
    {
        ValidarId(idPlanGuardia, nameof(idPlanGuardia));

        var plan = await ObtenerEntidadExistenteAsync(idPlanGuardia);

        if (EsEstadoAprobado(plan.IdEstadoNavigation))
        {
            throw new InvalidOperationException(
                "No se puede eliminar un plan aprobado.");
        }

        await _repository.EliminarAsync(plan);
    }

    // =========================================================
    // APROBAR PLAN
    // =========================================================

    public async Task<VistaPlanGuardia> AprobarAsync(
        int idPlanGuardia,
        int idPersonaAprobador)
    {
        ValidarId(idPlanGuardia, nameof(idPlanGuardia));
        ValidarId(idPersonaAprobador, nameof(idPersonaAprobador));

        var plan = await ObtenerEntidadExistenteAsync(idPlanGuardia);

        if (EsEstadoAprobado(plan.IdEstadoNavigation))
        {
            throw new InvalidOperationException(
                "El plan ya está aprobado.");
        }

        if (plan.DetallePlanGuardia.Count == 0)
        {
            throw new InvalidOperationException(
                "No se puede aprobar un plan sin detalles.");
        }

        var autorizado =
            await _repository.EsResponsableAprobacionAsync(
                idPersonaAprobador);

        if (!autorizado)
        {
            throw new InvalidOperationException(
                "La persona no está configurada como responsable de aprobación.");
        }

        var estadoAprobado =
            await _repository.ObtenerOCrearEstadoAsync(
                EstadoAprobadoCodigo,
                "Aprobado",
                "Plan de guardia aprobado",
                "#16A34A",
                "check-circle",
                idPersonaAprobador);

        plan.IdEstado = estadoAprobado.IdEstado;
        plan.IdEstadoNavigation = estadoAprobado;
        plan.IdPersonaAprobo = idPersonaAprobador;
        plan.FechaHoraAprobacion = DateTime.Now;
        plan.IdPersonaActualiza = idPersonaAprobador;
        plan.FechaHoraActualizado = DateTime.Now;

        await _repository.GuardarCambiosAsync();

        return await ObtenerPorIdAsync(idPlanGuardia);
    }

    // =========================================================
    // PLANIFICACIÓN AUTOMÁTICA POR PERÍODO
    // Recibe fecha inicial y fecha final y planifica todos los
    // días entre ambas (inclusive).
    // =========================================================

    public async Task<VistaPlanGuardia> PlanificarAutomaticamenteAsync(
        PlanGuardiaPlanificacionAutomaticaDto dto,
        int idPersonaActor)
    {
        ArgumentNullException.ThrowIfNull(dto);

        ValidarId(idPersonaActor, nameof(idPersonaActor));

        var fechaInicial = dto.FechaInicial.Date;
        var fechaFin = dto.FechaFin.Date;

        if (dto.FechaInicial == default || dto.FechaFin == default)
        {
            throw new ArgumentException(
                "La fecha inicial y la fecha final son obligatorias.");
        }

        if (fechaInicial > fechaFin)
        {
            throw new ArgumentException(
                "La fecha inicial no puede ser posterior a la fecha final.");
        }

        if ((fechaFin - fechaInicial).TotalDays > 366)
        {
            throw new ArgumentException(
                "El período a planificar no puede superar un año.");
        }

        if (dto.CantidadPersonasPorGrupoPorDia < 1 ||
            dto.CantidadPersonasPorGrupoPorDia > 20)
        {
            throw new ArgumentException(
                "La cantidad de personas por grupo y día debe estar entre 1 y 20.");
        }

        ValidarHora(dto.HoraInicio, nameof(dto.HoraInicio));
        ValidarHora(dto.HoraFin, nameof(dto.HoraFin));

        // -----------------------------------------------------
        // 1. Consultar feriados del período.
        //
        // Si falla la consulta o no hay registros, continuar
        // sin considerar feriados.
        // -----------------------------------------------------

        var feriados = new HashSet<DateTime>();

        try
        {
            var feriadosRegistrados =
                await _repository.ObtenerFeriadosPeriodoAsync(
                    fechaInicial,
                    fechaFin);

            feriados = feriadosRegistrados
                .Select(f => f.Fecha.Date)
                .ToHashSet();

            if (feriados.Count == 0)
            {
                _logger.LogInformation(
                    "No hay feriados registrados para el período {FechaInicial:yyyy-MM-dd} - {FechaFin:yyyy-MM-dd}. " +
                    "Se continuará sin ponderación de feriados.",
                    fechaInicial,
                    fechaFin);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(
                ex,
                "No se pudieron consultar los feriados del período {FechaInicial:yyyy-MM-dd} - {FechaFin:yyyy-MM-dd}. " +
                "Se continuará con la planificación sin considerar feriados.",
                fechaInicial,
                fechaFin);
        }

        // -----------------------------------------------------
        // 2. Comprobar que no haya otro plan solapado.
        //
        // El mensaje identifica exactamente qué días del período
        // ya están cubiertos por planes existentes.
        // -----------------------------------------------------

        var diasSolapados =
            await _repository.ObtenerFechasSolapadasAsync(
                fechaInicial,
                fechaFin);

        if (diasSolapados.Count > 0)
        {
            var listadoDias = string.Join(
                ", ",
                diasSolapados
                    .OrderBy(f => f)
                    .Select(f => f.ToString("yyyy-MM-dd")));

            throw new InvalidOperationException(
                "Ya existe un plan de guardia con la planificación de " +
                $"algunos días del período solicitado: {listadoDias}. " +
                "No se creó el plan automático.");
        }

        // -----------------------------------------------------
        // 3. Obtener integrantes activos de grupos de guardia.
        // -----------------------------------------------------

        var miembros =
            await _repository.ObtenerMiembrosGrupoPeriodoAsync(
                fechaInicial,
                fechaFin);

        if (miembros.Count == 0)
        {
            throw new InvalidOperationException(
                "No existen integrantes activos de grupos de guardia en ese período.");
        }

        var idsPersonas = miembros
            .Select(m => m.IdPersona)
            .Distinct()
            .ToArray();

        // -----------------------------------------------------
        // 4. Obtener afectaciones que se solapan con el período.
        // -----------------------------------------------------

        var afectaciones =
            await _repository.ObtenerAfectacionesPeriodoAsync(
                idsPersonas,
                fechaInicial,
                fechaFin);

        // -----------------------------------------------------
        // 5. Historial aprobado de los 12 meses anteriores.
        // -----------------------------------------------------

        var fechaInicioHistorial = fechaInicial.AddMonths(-12);
        var fechaFinHistorial = fechaInicial.AddDays(-1);

        var historial =
            await _repository.ObtenerHistorialAprobadoAsync(
                fechaInicioHistorial,
                fechaFinHistorial);

        var historialPorPersona = historial
            .GroupBy(h => h.IdPersona)
            .ToDictionary(g => g.Key, g => g.ToList());

        var mesAnteriorInicial = fechaInicial.AddMonths(-1);
        var mesAnteriorFinal = fechaInicial.AddDays(-1);

        var mismoMesAnioAnteriorInicial = fechaInicial.AddYears(-1);
        var mismoMesAnioAnteriorFinal =
            fechaInicial.AddYears(-1).AddMonths(1).AddDays(-1);

        // -----------------------------------------------------
        // 6. Preparar contadores para repartir la carga.
        // -----------------------------------------------------

        var cargaMes = new Dictionary<int, int>();
        var cargaFinesSemana = new Dictionary<int, int>();
        var cargaFeriados = new Dictionary<int, int>();

        var personasAsignadasPorFecha =
            new HashSet<(DateTime Fecha, int Persona)>();

        var detalles = new List<DetallePlanGuardia>();

        // -----------------------------------------------------
        // 7. Recorrer cada día del período y cada grupo con
        // integrantes vigentes.
        //
        // Regla de sexo: las personas de sexo FEMENINO no se
        // asignan a guardias de fines de semana (sábado/domingo).
        // -----------------------------------------------------

        for (var fecha = fechaInicial.Date;
             fecha <= fechaFin.Date;
             fecha = fecha.AddDays(1))
        {
            var esFinDeSemanaDelDia = EsFinDeSemana(fecha);

            var gruposDelDia = miembros
                .Where(m =>
                    m.FechaInicio.Date <= fecha &&
                    (!m.FechaFin.HasValue ||
                     m.FechaFin.Value.Date >= fecha))
                .GroupBy(m => m.IdGrupoGuardia)
                .OrderBy(g => g.Key)
                .ToList();

            if (gruposDelDia.Count == 0)
            {
                throw new InvalidOperationException(
                    $"No hay grupos con integrantes activos el {fecha:yyyy-MM-dd}.");
            }

            foreach (var grupo in gruposDelDia)
            {
                var miembrosGrupo = grupo
                    .GroupBy(m => m.IdPersona)
                    .Select(g => g.First())
                    .ToList();

                for (var i = 0;
                     i < dto.CantidadPersonasPorGrupoPorDia;
                     i++)
                {
                    // No asignar a quien tenga una afectación,
                    // cumpla años o ya tenga guardia ese día.
                    // En fines de semana tampoco se asigna a
                    // personas de sexo femenino.
                    var candidatos = miembrosGrupo
                        .Where(m =>
                            !personasAsignadasPorFecha.Contains(
                                (fecha, m.IdPersona)) &&
                            !EstaAfectada(
                                afectaciones,
                                m.IdPersona,
                                fecha) &&
                            !EsCumpleanos(
                                m.CarnetIdentidad,
                                fecha) &&
                            !(esFinDeSemanaDelDia &&
                              EsMujer(m.Sexo)))
                        .ToList();

                    if (candidatos.Count == 0)
                    {
                        var nombreGrupo = miembrosGrupo
                            .Select(m => m.GrupoNombre)
                            .FirstOrDefault() ?? grupo.Key.ToString();

                        var motivoSexo = esFinDeSemanaDelDia &&
                            miembrosGrupo.Any(m => !EsMujer(m.Sexo))
                                ? string.Empty
                                : " Las mujeres no se planifican en fines de semana.";

                        throw new InvalidOperationException(
                            $"No hay suficientes personas disponibles del grupo " +
                            $"'{nombreGrupo}' el día {fecha:yyyy-MM-dd}." +
                            motivoSexo +
                            " No se creó el plan.");
                    }

                    // Orden de equidad:
                    // 1. Menor cantidad de guardias del mes anterior.
                    // 2. Menor cantidad en el mismo mes del año anterior.
                    // 3. Menos fines de semana asignados en el mes actual.
                    // 4. Menos feriados asignados en el mes actual.
                    // 5. Menor carga total del mes actual.
                    // 6. Menor carga histórica de los últimos 12 meses.
                    var seleccionado = candidatos
                        .OrderBy(m => ContarPeriodo(
                            historialPorPersona,
                            m.IdPersona,
                            mesAnteriorInicial,
                            mesAnteriorFinal))
                        .ThenBy(m => ContarPeriodo(
                            historialPorPersona,
                            m.IdPersona,
                            mismoMesAnioAnteriorInicial,
                            mismoMesAnioAnteriorFinal))
                        .ThenBy(m => EsFinDeSemana(fecha)
                            ? ObtenerContador(
                                cargaFinesSemana,
                                m.IdPersona)
                            : 0)
                        .ThenBy(m => feriados.Contains(fecha)
                            ? ObtenerContador(
                                cargaFeriados,
                                m.IdPersona)
                            : 0)
                        .ThenBy(m => ObtenerContador(
                            cargaMes,
                            m.IdPersona))
                        .ThenBy(m => ObtenerHistorialTotal(
                            historialPorPersona,
                            m.IdPersona))
                        .ThenBy(m => ObtenerHistorialFinesSemana(
                            historialPorPersona,
                            m.IdPersona))
                        .ThenBy(m => m.IdPersona)
                        .First();

                    var ahora = DateTime.Now;

                    detalles.Add(new DetallePlanGuardia
                    {
                        IdGrupoGuardia = seleccionado.IdGrupoGuardia,
                        IdPersona = seleccionado.IdPersona,
                        FechaGuardia = fecha,
                        HoraInicio = dto.HoraInicio,
                        HoraFin = dto.HoraFin,
                        Observacion = null,
                        IdPersonaCrea = idPersonaActor,
                        FechaHoraCreado = ahora,
                        FechaHoraActualizado = ahora
                    });

                    personasAsignadasPorFecha.Add(
                        (fecha, seleccionado.IdPersona));

                    Incrementar(cargaMes, seleccionado.IdPersona);

                    if (EsFinDeSemana(fecha))
                    {
                        Incrementar(
                            cargaFinesSemana,
                            seleccionado.IdPersona);
                    }

                    if (feriados.Contains(fecha))
                    {
                        Incrementar(
                            cargaFeriados,
                            seleccionado.IdPersona);
                    }
                }
            }
        }

        // -----------------------------------------------------
        // 8. Crear el plan en estado Borrador.
        // No se guarda nada si la generación falla en un día.
        // -----------------------------------------------------

        var borrador = await ObtenerEstadoBorradorAsync(idPersonaActor);
        var fechaHoraCreado = DateTime.Now;

        var plan = new PlanGuardiaEntity
        {
            FechaInicial = fechaInicial,
            FechaFin = fechaFin,
            IdEstado = borrador.IdEstado,
            IdEstadoNavigation = borrador,
            IdPersonaCrea = idPersonaActor,
            Observacion = NormalizarTexto(dto.Observacion, 1000),
            FechaHoraCreado = fechaHoraCreado,
            FechaHoraActualizado = fechaHoraCreado
        };

        foreach (var detalle in detalles)
        {
            plan.DetallePlanGuardia.Add(detalle);
        }

        await _repository.CrearAsync(plan);

        _logger.LogInformation(
            "Plan {IdPlanGuardia} generado para el período {FechaInicial:yyyy-MM-dd} - " +
            "{FechaFin:yyyy-MM-dd}, con {CantidadAsignaciones} asignaciones.",
            plan.IdPlanGuardia,
            fechaInicial,
            fechaFin,
            detalles.Count);

        return await ObtenerPorIdAsync(plan.IdPlanGuardia);
    }

    // =========================================================
    // MÉTODOS PRIVADOS
    // =========================================================

    private async Task<PlanGuardiaEntity> ObtenerEntidadExistenteAsync(
        int idPlanGuardia)
    {
        return await _repository.ObtenerEntidadPorIdAsync(idPlanGuardia)
            ?? throw new KeyNotFoundException(
                $"No existe el plan de guardia con ID {idPlanGuardia}.");
    }

    private async Task<EstadoParte> ObtenerEstadoBorradorAsync(
        int idPersona)
    {
        return await _repository.ObtenerOCrearEstadoAsync(
            EstadoBorradorCodigo,
            "Borrador",
            "Plan de guardia pendiente de aprobación",
            "#64748B",
            "file-pen",
            idPersona);
    }

    private async Task ValidarSolapamientoAsync(
        DateTime fechaInicial,
        DateTime fechaFin,
        int? excluirIdPlanGuardia = null)
    {
        if (await _repository.ExisteSolapamientoAsync(
                fechaInicial,
                fechaFin,
                excluirIdPlanGuardia))
        {
            throw new InvalidOperationException(
                "Ya existe otro plan de guardia que se solapa con ese período.");
        }
    }

    private static DetallePlanGuardia CrearDetalle(
        DetallePlanGuardiaDto dto,
        int idPersonaActor,
        PlanGuardiaEntity plan,
        DateTime ahora)
    {
        return new DetallePlanGuardia
        {
            IdPlanGuardiaNavigation = plan,
            IdGrupoGuardia = dto.IdGrupoGuardia,
            IdPersona = dto.IdPersona,
            FechaGuardia = dto.FechaGuardia.Date,
            HoraInicio = dto.HoraInicio,
            HoraFin = dto.HoraFin,
            Observacion = NormalizarTexto(dto.Observacion, 500),
            IdPersonaCrea = idPersonaActor,
            FechaHoraCreado = ahora,
            FechaHoraActualizado = ahora
        };
    }

    private static void ValidarDetalles(
        List<DetallePlanGuardiaDto>? detalles,
        DateTime fechaInicial,
        DateTime fechaFin)
    {
        if (detalles is null || detalles.Count == 0)
        {
            throw new ArgumentException(
                "Debe incluir al menos un detalle en el plan de guardia.");
        }

        foreach (var detalle in detalles)
        {
            if (detalle.IdGrupoGuardia <= 0)
            {
                throw new ArgumentException(
                    "Cada detalle debe contener un grupo de guardia válido.");
            }

            if (detalle.IdPersona <= 0)
            {
                throw new ArgumentException(
                    "Cada detalle debe contener una persona válida.");
            }

            if (detalle.FechaGuardia == default ||
                detalle.FechaGuardia.Date < fechaInicial.Date ||
                detalle.FechaGuardia.Date > fechaFin.Date)
            {
                throw new ArgumentException(
                    "La fecha de cada detalle debe estar dentro del período del plan.");
            }

            ValidarHora(detalle.HoraInicio, nameof(detalle.HoraInicio));
            ValidarHora(detalle.HoraFin, nameof(detalle.HoraFin));

            NormalizarTexto(detalle.Observacion, 500);
        }

        var duplicados = detalles
            .GroupBy(d => new
            {
                d.IdPersona,
                Fecha = d.FechaGuardia.Date
            })
            .Any(g => g.Count() > 1);

        if (duplicados)
        {
            throw new ArgumentException(
                "Una misma persona no puede tener dos asignaciones en una misma fecha del plan.");
        }
    }

    private static void ValidarPeriodo(
        DateTime fechaInicial,
        DateTime fechaFin)
    {
        if (fechaInicial == default || fechaFin == default)
        {
            throw new ArgumentException(
                "Las fechas inicial y final son obligatorias.");
        }

        if (fechaInicial.Date > fechaFin.Date)
        {
            throw new ArgumentException(
                "La fecha inicial no puede ser posterior a la fecha final.");
        }
    }

    private static void ValidarId(int id, string nombre)
    {
        if (id <= 0)
        {
            throw new ArgumentException(
                $"El valor de {nombre} debe ser mayor que cero.");
        }
    }

    private static void ValidarHora(TimeSpan? hora, string nombre)
    {
        if (hora.HasValue &&
            (hora.Value < TimeSpan.Zero ||
             hora.Value >= TimeSpan.FromDays(1)))
        {
            throw new ArgumentException(
                $"El valor de {nombre} no es válido.");
        }
    }

    private static string? NormalizarTexto(
        string? valor,
        int longitudMaxima)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        var texto = valor.Trim();

        if (texto.Length > longitudMaxima)
        {
            throw new ArgumentException(
                $"El texto no puede superar los {longitudMaxima} caracteres.");
        }

        return texto;
    }

    private static bool EsEstadoAprobado(EstadoParte estado)
    {
        return string.Equals(
                   estado.Codigo,
                   EstadoAprobadoCodigo,
                   StringComparison.OrdinalIgnoreCase) ||
               string.Equals(
                   estado.Nombre,
                   "Aprobado",
                   StringComparison.OrdinalIgnoreCase);
    }

    // =========================================================
    // AFECTACIONES
    // =========================================================

    private static bool EstaAfectada(
        IReadOnlyList<VistaPersonasAfectaciones> afectaciones,
        int idPersona,
        DateTime fecha)
    {
        var inicioDia = fecha.Date;
        var finDia = inicioDia.AddDays(1).AddTicks(-1);

        return afectaciones.Any(a =>
            a.IdPersona == idPersona &&
            a.IdAfectacion.HasValue &&
            a.FechaHoraInicio.HasValue &&
            a.FechaHoraInicio.Value <= finDia &&
            (
                !a.FechaHoraFin.HasValue ||
                a.FechaHoraFin.Value >= inicioDia
            ) &&
            EsAfectacionBloqueante(a.EstadoNombre));
    }

    private static bool EsAfectacionBloqueante(string? estado)
    {
        if (string.IsNullOrWhiteSpace(estado))
        {
            return true;
        }

        return !estado.Contains(
                   "rechaz",
                   StringComparison.OrdinalIgnoreCase) &&
               !estado.Contains(
                   "cancel",
                   StringComparison.OrdinalIgnoreCase) &&
               !estado.Contains(
                   "anulad",
                   StringComparison.OrdinalIgnoreCase);
    }

    // =========================================================
    // CUMPLEAÑOS A PARTIR DEL CARNÉ DE IDENTIDAD
    //
    // Los primeros seis dígitos representan YYMMDD.
    // Ejemplo: 950204 -> 4 de febrero.
    // =========================================================

    private static bool EsCumpleanos(
        string? carnetIdentidad,
        DateTime fecha)
    {
        if (string.IsNullOrWhiteSpace(carnetIdentidad))
        {
            return false;
        }

        var carnet = carnetIdentidad.Trim();

        if (carnet.Length < 6)
        {
            return false;
        }

        var primerosSeis = carnet[..6];

        if (!primerosSeis.All(char.IsDigit))
        {
            return false;
        }

        if (!int.TryParse(primerosSeis.Substring(2, 2), out var mes) ||
            !int.TryParse(primerosSeis.Substring(4, 2), out var dia))
        {
            return false;
        }

        if (mes < 1 || mes > 12)
        {
            return false;
        }

        // Se usa un año bisiesto de referencia para admitir el 29 de febrero.
        var maximoDia = DateTime.DaysInMonth(2000, mes);

        if (dia < 1 || dia > maximoDia)
        {
            return false;
        }

        return fecha.Month == mes && fecha.Day == dia;
    }

    // =========================================================
    // FINES DE SEMANA
    // =========================================================

    private static bool EsFinDeSemana(DateTime fecha)
    {
        return fecha.DayOfWeek is
            DayOfWeek.Saturday or DayOfWeek.Sunday;
    }

    // =========================================================
    // SEXO
    //
    // Regla del módulo: las mujeres (sexo "Femenino") no se
    // planifican en fines de semana. Se aceptan las variantes
    // más comunes del catálogo de sexos de la base `persona`.
    // =========================================================

    private static bool EsMujer(string? sexo)
    {
        if (string.IsNullOrWhiteSpace(sexo))
        {
            return false;
        }

        var valor = sexo.Trim();

        return valor.Equals("F", StringComparison.OrdinalIgnoreCase) ||
               valor.Equals("Femenino", StringComparison.OrdinalIgnoreCase) ||
               valor.Equals("Mujer", StringComparison.OrdinalIgnoreCase) ||
               valor.Equals("Female", StringComparison.OrdinalIgnoreCase);
    }

    // =========================================================
    // HISTORIAL
    // =========================================================

    private static int ContarPeriodo(
        IReadOnlyDictionary<int, List<RegistroGuardiaHistoricaDto>> historial,
        int idPersona,
        DateTime fechaInicial,
        DateTime fechaFin)
    {
        if (!historial.TryGetValue(idPersona, out var registros))
        {
            return 0;
        }

        return registros.Count(r =>
            r.FechaGuardia.Date >= fechaInicial.Date &&
            r.FechaGuardia.Date <= fechaFin.Date);
    }

    private static int ObtenerHistorialTotal(
        IReadOnlyDictionary<int, List<RegistroGuardiaHistoricaDto>> historial,
        int idPersona)
    {
        return historial.TryGetValue(idPersona, out var registros)
            ? registros.Count
            : 0;
    }

    private static int ObtenerHistorialFinesSemana(
        IReadOnlyDictionary<int, List<RegistroGuardiaHistoricaDto>> historial,
        int idPersona)
    {
        if (!historial.TryGetValue(idPersona, out var registros))
        {
            return 0;
        }

        return registros.Count(r =>
            EsFinDeSemana(r.FechaGuardia));
    }

    // =========================================================
    // CONTADORES DE EQUILIBRIO DEL MES ACTUAL
    // =========================================================

    private static int ObtenerContador(
        IReadOnlyDictionary<int, int> contadores,
        int idPersona)
    {
        return contadores.TryGetValue(idPersona, out var cantidad)
            ? cantidad
            : 0;
    }

    private static void Incrementar(
        IDictionary<int, int> contadores,
        int idPersona)
    {
        contadores[idPersona] =
            contadores.TryGetValue(idPersona, out var cantidad)
                ? cantidad + 1
                : 1;
    }
}