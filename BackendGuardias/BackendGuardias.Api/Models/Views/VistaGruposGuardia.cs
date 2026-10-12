using System;
using System.Collections.Generic;

namespace BackendGuardias.Api.Models.Views;

public partial class VistaGruposGuardia
{
    public int IdGrupoGuardia { get; set; }

    public string GrupoNombre { get; set; } = null!;

    public string Iniciales { get; set; } = null!;

    public int? SubordinadoAIdGrupoGuardia { get; set; }

    public string? GrupoPadreNombre { get; set; }

    public string? GrupoPadreIniciales { get; set; }

    public string? Color { get; set; }

    public string? Icono { get; set; }

    public string? Observacion { get; set; }

    public bool? Activo { get; set; }

    public long CantidadPersonas { get; set; }

    public DateTime FechaHoraCreado { get; set; }

    public DateTime FechaHoraActualizado { get; set; }
}
