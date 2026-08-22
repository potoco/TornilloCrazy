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
