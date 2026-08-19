using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataServicio.Tabla;

[Table("LisPreProConfiguracion")]
public class LisPreProConfiguracionTbl
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int LisPreProConfiguracionId { get; set; }

    [MaxLength(15)] public string? ClaveLocal { get; set; }
    [MaxLength(15)] public string? ClaveProveedor { get; set; }

    public int? ListaPrecioProveedorId { get; set; }
    public ListaPrecioProveedorTbl? ListaPrecioProveedor { get; set; }
}
