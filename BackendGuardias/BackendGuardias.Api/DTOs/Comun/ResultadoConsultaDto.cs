namespace BackendGuardias.Api.DTOs.Comun;

public sealed class ResultadoConsultaDto<T>
{
    public int Total { get; init; }

    public IReadOnlyList<T> Data { get; init; } =
        Array.Empty<T>();
}