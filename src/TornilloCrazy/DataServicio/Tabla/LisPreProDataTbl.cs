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


    public int? ListaPrecioProveedorId { get; set; }
    public ListaPrecioProveedorTbl? ListaPrecioProveedor { get; set; }
}
