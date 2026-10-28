using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataServicio.Tabla;

[Table("Venta")]
public class VentaTbl
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int VentaId { get; set; }
    public int ClientId { get; set; }
    public decimal ClienteDescuentoPorcentaje { get; set; }
    public decimal ClienteDescuentoValor { get; set; }
    public decimal Iva { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Total { get; set; }
    public DateTime Fecha { get; set; }
    public ICollection<VentaStockTbl> VentaStocks { get; set; } = new List<VentaStockTbl>();
}

[Table("VentaStock")]
public class VentaStockTbl
{
    public int VentaId { get; set; }
    public int StockControlId { get; set; }
    
    // Navegación
    public VentaTbl Ventas { get; set; } = null!;
    public StockControlTbl StockControl { get; set; } = null!;
}