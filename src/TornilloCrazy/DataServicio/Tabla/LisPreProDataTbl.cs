using Microsoft.Data.SqlTypes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataServicio.Tabla;

[Table("LisPreProData")]
public class LisPreProDataTbl
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int LisPreProDataId { get; set; }

    public string? LineaCruda { get; set; }
    [MaxLength(254)] public string? Descripcion1 { get; set; }
    [MaxLength(254)] public string? Descripcion2 { get; set; }
    [MaxLength(254)] public string? Descripcion3 { get; set; }
    [MaxLength(254)] public string? Descripcion4 { get; set; }
    [MaxLength(254)] public string? Descripcion5 { get; set; }
    [MaxLength(254)] public string? Descripcion6 { get; set; }
    [Column(TypeName = "float")] public float Valor1 { get; set; }
    [Column(TypeName = "float")] public float Valor2 { get; set; }
    [Column(TypeName = "float")] public float Valor3 { get; set; }

    /// <summary>
    /// EstadoRevisionIA 
    /// 1 = Revisando, 
    /// 2 = Aprobado, 
    /// 3 = Rechazado, 
    /// 4 = El item ya existe no debe revisar con IA,  
    /// colocar en 0 para reprocesar
    /// </summary>
    public int EstadoRevisionIA { get; set; } = 0; 
    public DateTime? FechaRevisionIA { get; set; } // Fecha de revisión por la IA

    public int? ProductoId { get; set; }
    public ProductoTbl? Producto { get; set; }

    public int? ListaPrecioProveedorId { get; set; }
    public ListaPrecioProveedorTbl? ListaPrecioProveedor { get; set; }

    [NotMapped] public string? AtributosJson { get; set; }
    [NotMapped] public string? SinonimosJson { get; set; }
    [NotMapped] public string? UsosJson { get; set; }
    [NotMapped] public string? RubrosJson { get; set; }
    [NotMapped] public string? NombreCanonico { get; set; }
    [NotMapped] public string? TextoVectorial { get; set; }
    [NotMapped] public SqlVector<float> VectorEmbedding { get; set; }
    [NotMapped] public string? JsonRaw { get; set; } 
}
