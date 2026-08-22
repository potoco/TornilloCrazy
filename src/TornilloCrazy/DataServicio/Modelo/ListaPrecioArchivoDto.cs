namespace DataServicio.Modelo;

public sealed record ListaPrecioArchivoDto(
    int ListaPrecioProveedorId,
    int ProveedorId,
    DateTime? FecIngreso,
    string? NombreArchivo,
    string? NombreArchivoOriginal);
