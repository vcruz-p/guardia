using System;
using System.Collections.Generic;

namespace BackendGuardias.Api.Models.Views;

public partial class VistaPersonasAfectaciones
{
    public int IdPersona { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Apellidos { get; set; }

    public string? NombreCompleto { get; set; }

    public string? CarnetIdentidad { get; set; }

    public int? IdUnidadOrganizativa { get; set; }

    public string? UnidadOrganizativaNombre { get; set; }

    public string? UnidadOrganizativaDescripcion { get; set; }

    public int? IdCargo { get; set; }

    public string? CargoDescripcion { get; set; }

    public int? IdGrado { get; set; }

    public string? GradoDescripcion { get; set; }

    public string? OrigenAfectacion { get; set; }

    public int? IdAfectacion { get; set; }

    public int? IdTipoAfectacion { get; set; }

    public string? AfectacionNombre { get; set; }

    public string? AfectacionDescripcion { get; set; }

    public string? AfectacionColor { get; set; }

    public byte[]? AfectacionIcono { get; set; }

    public string? AfectacionObservacion { get; set; }

    public DateTime? FechaHoraRegistro { get; set; }

    public DateTime? FechaHoraInicio { get; set; }

    public DateTime? FechaHoraFin { get; set; }

    public int? IdEstado { get; set; }

    public string? EstadoNombre { get; set; }

    public string? EstadoDescripcion { get; set; }

    public string? EstadoColor { get; set; }

    public bool? EstadoAlerta { get; set; }

    public string? CausaRechazo { get; set; }

    public int? Vigente { get; set; }
}
