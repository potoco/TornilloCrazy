using Microsoft.Data.SqlTypes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataServicio.Tabla;

[Table("ProveedorMaestro")]
public class ProveedorMaestroTbl
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProveedorMaestroId { get; set; }
    [MaxLength(254)] public string? Descripcion1 { get; set; }

    [MaxLength(254)] public string NombreCanonico { get; set; } = "";
    public string? AtributosJson { get; set; }
    public string? SinonimosJson { get; set; }
    public string? UsosJson { get; set; }
    public string? RubrosJson { get; set; }
    public string? TextoVectorial { get; set; }
    public string? JsonRaw { get; set; }    

    [Column(TypeName = "vector(1536)")]
    public SqlVector<float>? VectorEmbedding { get; set; }

    /// <summary>
    /// EstadoRevisionIA 
    /// 0 = Para revisar, 
    /// 1 = Revisando, 
    /// 2 = Aprobado, 
    /// 3 = Rechazado, 
    /// 4 = El item ya existe no debe revisar con IA,  
    /// colocar en 0 para reprocesar
    /// </summary>
    public int EstadoRevisionIA { get; set; } = 0;
    public DateTime? FechaRevisionIA { get; set; } // Fecha de revisión por la IA
    public DateTime? FechaSeleccionProducto { get; set; } // Fecha de selección del producto

    [Column(TypeName = "nvarchar(max)")]
    public string? DescripcionDetallada { get; set; }

    public int ProveedorId { get; set; }
    public ProveedorTbl Proveedor { get; set; } = null!;

    public int? ProductoId { get; set; } 
    public ProductoTbl? Producto { get; set; }

    [MaxLength(500)] public string ErrorMensaje { get; set; } = string.Empty;
}
