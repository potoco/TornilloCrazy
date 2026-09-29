using DataServicio.Migrations;
using DataServicio.Modelo;
using DataServicio.Servicio;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

namespace TornilloWeb.Servicio;

public class LaburandoService : BackgroundService
{
    const int _Chunk = 15;
    const int _tiempoEspera = 2000; // Tiempo de espera en milisegundos (2 segundos)
    private readonly ClienteWebPotoco _clienteWebPotoco;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LaburandoService> _logger;
    public LaburandoService(IServiceProvider serviceProvider, ILogger<LaburandoService> logger, ClienteWebPotoco clienteWebPotoco)
    {
        _serviceProvider = serviceProvider;
        _clienteWebPotoco = clienteWebPotoco;
        _logger = logger;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var formato = "dd/MM/yyyy";
        var cultura = CultureInfo.InvariantCulture;
        var tarea1 = ReActivar(ct => ProcesarListaPrecio(formato, cultura, ct), stoppingToken);
        var tarea2 = ReActivar(ct => BuscarIADescripciones(formato, cultura, ct), stoppingToken);
        var tarea3 = ReActivar(ct => BuscarMaestro(formato, cultura, ct), stoppingToken);
        await Task.WhenAll(tarea1, tarea2, tarea3);
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
            if (listaSinProcesar == null || !listaSinProcesar.Any())
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
        var instruccionesTextoHelper = sco.ServiceProvider.GetRequiredService<InstruccionesTextoHelper>();
        while (!ct.IsCancellationRequested)
        {
            var preciosAprocesar = await proveedorServicio.ObtenerItemsProveedor();
            if (preciosAprocesar == null || !preciosAprocesar.Any())
            {
                await Task.Delay(_tiempoEspera, ct);
                continue;
            }

            var lotes = preciosAprocesar.Chunk(_Chunk);
            int totalLotes = lotes.Count();
            foreach (var loteActual in lotes)
            {
                var ids = string.Join(", ", loteActual.Select(y => y.ProveedorMaestroId).ToList());
                _logger.LogInformation($"BuscarIADescripciones -> Procesando Lotes de ProveedorMaestroId {ids}");
                try
                {
                    await proveedorServicio.MarcarItemsEnProceso(loteActual.ToList());
                    
                    await Task.Delay(1200, ct); // esperar 1200 milisegundos antes de la siguiente iteración
                    var modeloBuscarIA = await instruccionesTextoHelper.CrearModeloBuscarDescripcion(loteActual.ToList());
                    await _clienteWebPotoco.BuscarDescripcionesIA(modeloBuscarIA);
                    await Task.Delay(1200, ct); // esperar 1200 milisegundos antes de la siguiente iteración
                    var modeloVector = await instruccionesTextoHelper.CrearModeloVector(modeloBuscarIA);
                    await _clienteWebPotoco.CrearVector(modeloVector);
                    await proveedorServicio.ActualizaryMarcarComoProcesados(modeloVector.PreciosRevisar.ToList());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"BuscarIADescripciones -> Error en procesando Lotes de ProveedorMaestroId {ids}");
                }
            }

            await Task.Delay(_tiempoEspera, ct); // esperar 2 segundos antes de la siguiente iteración
        }
    }
    private async Task BuscarMaestro(string formato, CultureInfo cultura, CancellationToken ct) 
    {
        using var sco = _serviceProvider.CreateAsyncScope();
        var proveedorServicio = sco.ServiceProvider.GetRequiredService<ProveedorService>();
        var productoServicio = sco.ServiceProvider.GetRequiredService<ProductoService>();
        while (!ct.IsCancellationRequested)
        {
            var itemPreciosSinRevisar = await proveedorServicio.ObtenerItemsSinRevevisionParaMaestro();
            if (itemPreciosSinRevisar == null || !itemPreciosSinRevisar.Any())
            {
                await Task.Delay(_tiempoEspera, ct);
                continue; // No hay items sin revisar, continuar con la siguiente iteración
            }
            var listaIdParaActualizar = itemPreciosSinRevisar.Select(x => x.ProveedorMaestroId).ToList();
            var nuevoMaestro = new List<ListaRevisionProductoMaestro>();
            while (itemPreciosSinRevisar.Count > 0)
            {

                // 1. Tomar el elemento representante del grupo
                var item = itemPreciosSinRevisar[0];
                itemPreciosSinRevisar.RemoveAt(0);

                // 2. Buscar todos los que son casi idénticos (>= 0.98)
                var similares = itemPreciosSinRevisar
                    .Where(z => VectorHelper.Similitud(z.Vector, item.Vector) >= 0.98f)
                    .Select(x=>x.ProveedorMaestroId).ToList();
                if(similares != null && similares.Count > 0)
                {
                    item.ProveedorMaestroIdSimilares.AddRange(similares);
                    foreach (var sim in similares)
                    {
                        itemPreciosSinRevisar.RemoveAll(x => x.ProveedorMaestroId == sim)   ;
                    }

                }
                nuevoMaestro.Add(item);
            }
            if (nuevoMaestro.Count > 0)
            {
                var lotes = nuevoMaestro.Chunk(_Chunk);

                foreach (var loteActual in lotes)
                {
                    var ids = string.Join(", ", loteActual.Select(y => y.ProveedorMaestroId).ToList());

                    try
                    {
                        await productoServicio.CrearProductoAsync(loteActual.ToList());

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"BuscarMaestro -> Procesando Lotes de ProveedorMaestroId {ids}");

                    }
                }

            }
        }
    }
}
