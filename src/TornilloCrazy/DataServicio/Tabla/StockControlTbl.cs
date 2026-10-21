using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataServicio.Tabla;

[Table("StockControl")]
public class StockControlTbl
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int StockControlId { get; set; }
    public DateTime Fecha { get; set; }
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public string CodigoAlteracion { get; set; } = "";
}