using Microsoft.Data.SqlTypes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataServicio.Tabla;

[Table("Producto")]
public class ProductoTbl
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProductoId { get; set; }
    [MaxLength(200)] public string Nombre { get; set; } = "";
    [MaxLength(240)] public string Rubros { get; set; } = "";
    [MaxLength(240)] public string Sinonimos { get; set; } = "";
    [MaxLength(30)] public string? Codigo { get; set; }
    public int Cantidad { get; set; } = 0;
    public decimal PrecioUnitario { get; set; } = 0;
    public int Estado { get; set; } = 0;

    public string? AtributosJson { get; set; }
    public string? SinonimosJson { get; set; }
    public string? UsosJson { get; set; }
    public string? RubrosJson { get; set; }
    public string? TextoVectorial { get; set; }

    [Column(TypeName = "vector(1536)")]
    public SqlVector<float>? VectorEmbedding { get; set; }
    public string? JsonRaw { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? DescripcionDetallada { get; set; }

    public virtual ICollection<ProveedorMaestroTbl> ProveedorMaestros { get; set; } = new List<ProveedorMaestroTbl>();  

}
