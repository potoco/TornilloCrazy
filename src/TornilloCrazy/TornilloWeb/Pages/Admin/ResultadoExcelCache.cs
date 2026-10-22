using TornilloWeb.Servicio;

namespace TornilloWeb.Pages.Admin;

public sealed class ResultadoExcelCache
{
    public List<ProductoFromExcelDto> Items { get; init; } = [];
    public string? NombreOriginalArchivo { get; init; }
    public string? NombreArchivoGuardado { get; init; }
    public string? FullPathArchivoGuardado { get; init; }
}

