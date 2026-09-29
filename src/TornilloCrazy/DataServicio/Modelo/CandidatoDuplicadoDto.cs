using System;
using System.Collections.Generic;
using System.Text;

namespace DataServicio.Modelo;

public class CandidatoDuplicadoDto
{
    public int ProductoId { get; set; }
    public int ProveedorId { get; set; }
    public string NombreCanonico { get; set; } = string.Empty;
    public string? AtributosJson { get; set; }
    public double Distancia { get; set; }
}

public class CandidatoDuplicadoConProveedorDto 
{
    public string ProveedorNombre { get; set; } = string.Empty;
    public string Precio { get; set; } = string.Empty;
}

public sealed class ProductoResultadoDto
{
    public string NombreProducto { get; set; } = string.Empty;
    public int ProductoId { get; set; }
    public int ProveedorMaestroId { get; set; }
    public string NombreCanonico { get; set; } = string.Empty;
    public string Proveedor { get; set; } = string.Empty;
    public int CodigoLista { get; set; }
    public DateTime FecIngreso { get; set; }
    public int LisPreProDataId { get; set; }
    public string TextoVectorial { get; set; } = string.Empty;
    public string LineaCruda { get; set; } = string.Empty;
    public double Distancia { get; set; }
    public string DescripcionDetallada { get; set; } = string.Empty;
    

}