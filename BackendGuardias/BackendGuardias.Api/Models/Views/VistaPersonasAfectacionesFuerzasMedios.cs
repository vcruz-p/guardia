using System;
using System.Collections.Generic;

namespace BackendGuardias.Api.Models.Views;

public partial class VistaPersonasAfectacionesFuerzasMedios
{
    public int IdPersona { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Apellidos { get; set; }

    public string? NombreCompleto { get; set; }

    public string? CarnetIdentidad { get; set; }

    public int? IdUnidadOrganizativa { get; set; }

    public string? UnidadOrganizativaNombre { get; set; }

    public string? UnidadOrganizativaDescripcion { get; set; }

    public int? CargoId { get; set; }

    public string? CargoDescripcion { get; set; }

    public int? GradoId { get; set; }

    public string? GradoDescripcion { get; set; }

    public int? IdAfectacionFuerzasMedios { get; set; }

    public int? IdTipoAfectacionFuerzasMedios { get; set; }

    public string? AfectacionNombre { get; set; }

    public string? AfectacionDescripcion { get; set; }

    public string? AfectacionColor { get; set; }

    public bool? AfectacionAlerta { get; set; }

    public string? AfectacionObservacion { get; set; }

    public DateTime? FechaHoraRegistro { get; set; }

    public DateTime? FechaHoraInicio { get; set; }

    public DateTime? FechaHoraFin { get; set; }

    public int? IdEstadoAfectacion { get; set; }

    public string? EstadoAfectacion { get; set; }

    public string? EstadoAfectacionDescripcion { get; set; }

    public string? EstadoAfectacionColor { get; set; }

    public bool? EstadoAfectacionAlerta { get; set; }

    public string? CausaRechazo { get; set; }

    public int AfectacionVigente { get; set; }
}
