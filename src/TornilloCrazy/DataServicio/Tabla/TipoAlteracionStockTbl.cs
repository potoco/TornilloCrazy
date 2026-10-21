using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataServicio.Tabla;

[Table("TipoAlteracionStock")]
public class TipoAlteracionStockTbl
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TipoAlteracionStockId { get; set; }
    [MaxLength(50)] public string Nombre { get; set; } = "";
    [MaxLength(2)] public string CodigoAlteracion { get; set; } = "";
    [MaxLength(150)] public string? Descripcion { get; set; } = "";
}
