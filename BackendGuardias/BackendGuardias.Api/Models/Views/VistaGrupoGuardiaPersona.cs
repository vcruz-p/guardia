using System;
using System.Collections.Generic;

namespace BackendGuardias.Api.Models.Views;

public partial class VistaGrupoGuardiaPersona
{
    public int IdGrupoGuardiaPersona { get; set; }

    public int IdGrupoGuardia { get; set; }

    public string GrupoNombre { get; set; } = null!;

    public string GrupoIniciales { get; set; } = null!;

    public int IdPersona { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Apellidos { get; set; }

    public string? PersonaNombreCompleto { get; set; }

    public string? CarnetIdentidad { get; set; }

    public int? IdCargo { get; set; }

    public string? CargoDescripcion { get; set; }

    public int? IdGrado { get; set; }

    public string? GradoDescripcion { get; set; }

    /// <summary>
    /// Sexo de la persona (p. ej. "Masculino" / "Femenino"),
    /// traído desde la base `persona` por la vista de la BD.
    /// </summary>
    public string? Sexo { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public bool? Activo { get; set; }

    public string? Observacion { get; set; }

    public int? IdPersonaCrea { get; set; }

    public int? IdPersonaActualiza { get; set; }

    public DateTime FechaHoraCreado { get; set; }

    public DateTime FechaHoraActualizado { get; set; }
}
