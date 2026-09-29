using Microsoft.Data.SqlTypes;

namespace DataServicio.Modelo;

public record ListaRevisionProductoMaestro(int ProveedorMaestroId, SqlVector<float>? Vector, List<int>? ProveedorMaestroIdSimilares);