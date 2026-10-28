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
    public decimal PrecioUnitario { get; set; } // se lleva el precio unitario del producto al momento de la alteración
    public string CodigoAlteracion { get; set; } = "";
    public ICollection<VentaStockTbl> VentaStocks { get; set; } = new List<VentaStockTbl>();

}
