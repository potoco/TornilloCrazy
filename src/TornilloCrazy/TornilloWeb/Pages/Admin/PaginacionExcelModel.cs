namespace TornilloWeb.Pages.Admin;

public sealed class PaginacionExcelModel
{
    public bool Mostrar { get; set; } = true;
    public string? LoteId { get; init; }
    public int CurrentPage { get; init; }
    public int TotalPages { get; init; }
    public string? FiltroDescripcion { get; init; }
    public IEnumerable<int> PaginasVisibles { get; init; } = [];
    public int DisplayCurrentPage => TotalPages == 0 ? 0 : CurrentPage;
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
    public bool MostrarNavegacion => TotalPages > 1;
}

