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
    [MaxLength(200)] public string Nombre { get; set; } = "";
    public string? AtributosJson { get; set; }
    public string? SinonimosJson { get; set; }
    public string? UsosJson { get; set; }
    public string? RubrosJson { get; set; }
    public string? TextoVectorial { get; set; }

    [Column(TypeName = "vector(1536)")]
    public SqlVector<float>? VectorEmbedding { get; set; }

    public int ProveedorId { get; set; }
    public ProveedorTbl Proveedor { get; set; } = null!;

    public int ProductoId { get; set; }
    public ProductoTbl Producto { get; set; } = null!;

}
