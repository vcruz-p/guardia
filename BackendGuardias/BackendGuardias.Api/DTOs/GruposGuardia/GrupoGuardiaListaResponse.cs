namespace BackendGuardias.Api.DTOs.GruposGuardia;

/// <summary>Respuesta paginable que contiene el total y los grupos de guardia.</summary>
public sealed record GrupoGuardiaListaResponse(
    int Total,
    IReadOnlyList<GrupoGuardiaDto> Data);
