using System.Data;
using DataServicio.Tabla;
using Microsoft.EntityFrameworkCore;

namespace DataServicio.Servicio;

public class VentaService(FerreteriaDbContext context)
{
    public sealed record Linea(int ProductoId, int Cantidad, decimal Precio);

    public async Task IngresarAsync(int clienteId, decimal ivaPorcentaje, IReadOnlyCollection<Linea> lineas)
    {
        if (lineas.Count == 0 || lineas.Count > 200 || lineas.Any(x => x.Cantidad <= 0) ||
            lineas.Select(x => x.ProductoId).Distinct().Count() != lineas.Count)
            throw new InvalidOperationException("El carrito debe contener entre 1 y 200 productos distintos con cantidades válidas.");
        if (ivaPorcentaje < 0 || ivaPorcentaje > 100)
            throw new InvalidOperationException("El IVA debe estar entre 0 y 100%.");
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var cliente = await context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.ClienteId == clienteId && c.Estado >= 0);
        if (cliente is null)
            throw new InvalidOperationException("Seleccione un cliente activo.");
        if (cliente.DescuentoPorcentaje < 0 || cliente.DescuentoPorcentaje > 100)
            throw new InvalidOperationException("El descuento del cliente debe estar entre 0 y 100%.");
        var ids = lineas.Select(x => x.ProductoId).ToArray();
        var productos = await context.Productos.Where(p => ids.Contains(p.ProductoId)).ToDictionaryAsync(p => p.ProductoId);
        var fecha = DateTime.Now;
        var movimientos = new List<(StockControlTbl Stock, decimal Subtotal, decimal Iva, decimal Descuento)>();
        foreach (var linea in lineas.OrderBy(x => x.ProductoId))
        {
            if (!productos.TryGetValue(linea.ProductoId, out var producto) || producto.Estado < 0)
                throw new InvalidOperationException("Uno de los productos ya no está disponible.");
            if (producto.Cantidad < linea.Cantidad)
                throw new InvalidOperationException($"Stock insuficiente para {producto.Nombre}. Disponible: {producto.Cantidad}.");
            if (producto.PrecioUnitario < 0)
                throw new InvalidOperationException($"El precio de {producto.Nombre} no es válido.");
            if (producto.PrecioUnitario != linea.Precio)
                throw new InvalidOperationException($"El precio de {producto.Nombre} cambió. Quite el producto y agréguelo nuevamente desde el buscador.");
            var subtotal = decimal.Round(producto.PrecioUnitario * linea.Cantidad, 2, MidpointRounding.AwayFromZero);
            var descuento = decimal.Round(subtotal * cliente.DescuentoPorcentaje / 100, 2, MidpointRounding.AwayFromZero);
            subtotal -= descuento;
            var iva = decimal.Round(subtotal * ivaPorcentaje / 100, 2, MidpointRounding.AwayFromZero);
            var stock = new StockControlTbl
            {
                ProductoId = producto.ProductoId, Cantidad = linea.Cantidad,
                PrecioUnitario = producto.PrecioUnitario, CodigoAlteracion = "EE", Fecha = fecha
            };
            context.StockControls.Add(stock);
            producto.Cantidad -= linea.Cantidad;
            movimientos.Add((stock, subtotal, iva, descuento));
        }
        var subtotalVenta = movimientos.Sum(m => m.Subtotal);
        var ivaVenta = movimientos.Sum(m => m.Iva);
        var venta = new VentaTbl
        {
            ClientId = clienteId,
            Fecha = fecha,
            ClienteDescuentoPorcentaje = cliente.DescuentoPorcentaje,
            ClienteDescuentoValor = movimientos.Sum(m => m.Descuento),
            SubTotal = subtotalVenta,
            Iva = ivaVenta,
            Total = subtotalVenta + ivaVenta,
            VentaStocks = movimientos.Select(m => new VentaStockTbl
            {
                StockControl = m.Stock
            }).ToList()
        };
        // EF resuelve las claves de la cabecera y los movimientos al guardar sus vínculos.
        context.Ventas.Add(venta);
        await context.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}
