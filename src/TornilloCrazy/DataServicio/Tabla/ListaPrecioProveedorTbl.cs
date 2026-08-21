using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataServicio.Tabla;

[Table("ListaPrecioProveedor")]
public class ListaPrecioProveedorTbl
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ListaPrecioProveedorId { get; set; }

    public DateTime? FecIngreso { get; set; }

    [MaxLength(200)] public string? Comentario { get; set; }
    [MaxLength(200)] public string? NombreArchivo { get; set; }
    [MaxLength(200)] public string? NombreArchivoOriginal { get; set; }

    public DateTime? FecProcesamiento { get; set; }

    public int? ProveedorId { get; set; }
    public ProveedorTbl? Proveedor { get; set; }

    public ICollection<LisPreProConfiguracionTbl>? LisPreProConfiguracion { get; set; }
    public ICollection<LisPreProDataTbl>? LisPreProData { get; set; }

}
