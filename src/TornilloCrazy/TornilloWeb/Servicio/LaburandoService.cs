using DataServicio.Migrations;
using DataServicio.Servicio;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System.Globalization;
using System.Text;

namespace TornilloWeb.Servicio;

public class LaburandoService : BackgroundService
{
    const int _tiempoEspera = 2000; // Tiempo de espera en milisegundos (2 segundos)
    private readonly IServiceProvider _serviceProvider;
    public LaburandoService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var formato = "dd/MM/yyyy";
        var cultura = CultureInfo.InvariantCulture;
        var tarea1 = ReActivar(ct => ProcesarListaPrecio(formato, cultura, ct), stoppingToken);
        var tarea2 = ReActivar(ct => BuscarIADescripciones(formato, cultura, ct), stoppingToken);
        await Task.WhenAll(tarea1, tarea2);
    }

    private async Task ReActivar(Func<CancellationToken, Task> revivir, CancellationToken ct)
    {

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await revivir(ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            await Task.Delay(5000, ct);
        }
    }

    private async Task ProcesarListaPrecio(string formato, CultureInfo cultura, CancellationToken ct)
    {
        using var sco = _serviceProvider.CreateAsyncScope();
        var procesarListaPrecioServicio = sco.ServiceProvider.GetRequiredService<ProcesarListaPrecioServicio>();
        var proveedorServicio = sco.ServiceProvider.GetRequiredService<ProveedorService>();
        while (!ct.IsCancellationRequested)
        {
            var listaSinProcesar = await proveedorServicio.ObtenerListaPrecioSinProcesarAsync();
            if(listaSinProcesar == null || !listaSinProcesar.Any())
            {
                await Task.Delay(_tiempoEspera, ct);
                continue;
            }

            // Obtener la primera lista de precios sin procesar
            var listaProveedorDto = listaSinProcesar.FirstOrDefault();
            if (listaProveedorDto == null)
            {
                await Task.Delay(_tiempoEspera, ct);
                continue;
            }
            
            int idPrecioLista = listaProveedorDto.ListaPrecioProveedorId;
            int proveedorId = listaProveedorDto.ProveedorId ?? 0;   

            var crudo = procesarListaPrecioServicio.BuildExcelCsv(listaProveedorDto.NombreArchivo, proveedorId);
            var nueva = new StringBuilder();
            using var reader = new StringReader(crudo.ToString());
            string? linea;
            bool seAgregaronLineas = false;
            while ((linea = reader.ReadLine()) != null)
            {
                var resultado = TextFilterHelper.ProcesarLineaValida(linea, precioMinimo: 30m, precioMaximo: 999_999_999m);
                if (resultado != null)
                {
                    await proveedorServicio.AgregarItemPrecioAsync(idPrecioLista, resultado.Descripcion, resultado.LineaCruda);
                    await proveedorServicio.GuardarItemMaestroAsync(resultado.Descripcion, proveedorId);
                    seAgregaronLineas = true;
                }
            }
            if (seAgregaronLineas)
            {
                await proveedorServicio.MarcarListaPrecioComoProcesadaAsync(idPrecioLista);
            }

            await Task.Delay(_tiempoEspera, ct); // esperar 2 segundos antes de la siguiente iteración
        }
    }
    private async Task BuscarIADescripciones(string formato, CultureInfo cultura, CancellationToken ct)
    {
        using var sco = _serviceProvider.CreateAsyncScope();
        var proveedorServicio = sco.ServiceProvider.GetRequiredService<ProveedorService>();
        while (!ct.IsCancellationRequested)
        {
            var preciosAprocesar = await proveedorServicio.ObtenerItemsProveedor();
            if (preciosAprocesar == null || !preciosAprocesar.Any())
            {
                await Task.Delay(_tiempoEspera, ct);
                continue;
            }

            var lotes = preciosAprocesar.Chunk(6);
            int totalLotes = lotes.Count();
            foreach (var loteActual in lotes)
            {
                try
                {
                    await proveedorServicio.MarcarItemsEnProceso(loteActual.ToList());
                    await Task.Delay(800, ct); // esperar 800 milisegundos antes de la siguiente iteración
                    await ClienteWebPotoco.BuscarDescripcionesIA(loteActual.ToList());
                    await Task.Delay(800, ct); // esperar 800 milisegundos antes de la siguiente iteración
                    await ClienteWebPotoco.CrearVector(loteActual.ToList());
                    await proveedorServicio.ActualizaryMarcarComoProcesados(loteActual.ToList());

                }
                catch (Exception ex)
                {
                    var s = ex.StackTrace;
                    Console.WriteLine(s);
                }
            }

            await Task.Delay(_tiempoEspera, ct); // esperar 2 segundos antes de la siguiente iteración
        }
    }


}
