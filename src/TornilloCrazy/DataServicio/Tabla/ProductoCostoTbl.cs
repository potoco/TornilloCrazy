using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataServicio.Tabla;

[Table("ProductoCosto")]
public class ProductoCostoTbl
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProductoCostoId { get; set; }
    public int ProductoId { get; set; }
    public decimal Costo { get; set; }
    public DateTime Fecha { get; set; }
    public string? Descripcion { get; set; } = "";
}