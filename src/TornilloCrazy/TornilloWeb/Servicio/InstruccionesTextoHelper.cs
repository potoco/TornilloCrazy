using DataServicio.Tabla;
using DocumentFormat.OpenXml.Presentation;
using Serilog;

namespace TornilloWeb.Servicio;


public class ModeloBuscarDescripcion
{
    public string InstruccionDeveloper { get; set; } = string.Empty;
    public string InstruccionUsuario { get; set; } = string.Empty;
    public List<ProveedorMaestroTbl> PreciosRevisar { get; set; } = new();
}

public class ModeloEmbeddingProducto
{
    public string[] InstruccionUsuario { get; set; } = Array.Empty<string>();
    public List<ProveedorMaestroTbl> PreciosRevisar { get; set; } = new();
}

public class InstruccionesTextoHelper
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<InstruccionesTextoHelper> _logger;
    private readonly Serilog.ILogger _log;

    private const string CarpetaTextos = "Servicio/Textos";
    private const string ArchivoDetalle = "intruccion_detalle.txt";
    private const string ArchivoBusUsuario = "intruccion_emb_BusUsuario.txt";
    private const string ArchivoProducto = "intruccion_emb_producto.txt";

    public InstruccionesTextoHelper(
        IWebHostEnvironment environment,
        ILogger<InstruccionesTextoHelper> logger)
    {
        _environment = environment;
        _logger = logger;
        _log = Log.ForContext<InstruccionesTextoHelper>();
        _log.Debug("InstruccionesTextoHelper inicializado con ContentRootPath: {ContentRootPath}", _environment.ContentRootPath);
    }

    public Task<string> ObtenerIntruccionDetalleAsync(CancellationToken ct = default)
    {
        _log.Information("Obteniendo instrucción de detalle.");
        return LeerArchivoAsync(ArchivoDetalle, ct);
    }

    public Task<string> ObtenerIntruccionEmbBusUsuarioAsync(CancellationToken ct = default)
    {
        _log.Information("Obteniendo instrucción de embedding para búsqueda de usuario.");
        return LeerArchivoAsync(ArchivoBusUsuario, ct);
    }

    public Task<string> ObtenerIntruccionEmbProductoAsync(CancellationToken ct = default)
    {
        _log.Information("Obteniendo instrucción de embedding para producto.");
        return LeerArchivoAsync(ArchivoProducto, ct);
    }


    public async Task<ModeloBuscarDescripcion> CrearModeloBuscarDescripcion(List<ProveedorMaestroTbl> preciosRevisar)
    {
        _log.Information("Creando ModeloBuscarDescripcion para {Cantidad} productos.", preciosRevisar.Count);

        ModeloBuscarDescripcion salida = new();

        var nuevosProductosarevisar = string.Empty;

        foreach (var rec in preciosRevisar)
        {
            int posicion = preciosRevisar.IndexOf(rec) + 1;
            string caracterFinal = posicion == preciosRevisar.Count ? "" : "\n";
            nuevosProductosarevisar += $"{posicion}. {rec.Descripcion1}{caracterFinal}";
        }

        salida.InstruccionDeveloper = await ObtenerIntruccionDetalleAsync();
        salida.InstruccionUsuario = nuevosProductosarevisar;
        salida.PreciosRevisar = preciosRevisar;
        _log.Information("ModeloBuscarDescripcion creado correctamente.");
        return salida;
    }
    public async Task<ModeloEmbeddingProducto> CrearModeloVector(ModeloBuscarDescripcion dataModelo)
    {
        _log.Information("Creando ModeloEmbeddingProducto para {Cantidad} productos.", dataModelo.PreciosRevisar.Count);
        ModeloEmbeddingProducto salida = new();

        var lisPreProDataTbls = dataModelo.PreciosRevisar;
        var instruccionesPre = await ObtenerIntruccionEmbProductoAsync();
        var inpusT = lisPreProDataTbls.Where(x => x.ErrorMensaje == "sin errores").OrderBy(x => x.ProveedorMaestroId).Select(x => $"{instruccionesPre}{x.TextoVectorial}").ToArray();

        salida.PreciosRevisar = dataModelo.PreciosRevisar;
        salida.InstruccionUsuario = inpusT; //$"{instruccionesPre}{string.Join("\n", inpusT)}";
        _log.Information("ModeloEmbeddingProducto creado con {Cantidad} entradas para embedding.", inpusT.Length);
        return salida;
    }


    private async Task<string> LeerArchivoAsync(string nombreArchivo, CancellationToken ct)
    {
        var ruta = Path.Combine(_environment.ContentRootPath, CarpetaTextos, nombreArchivo);
        _log.Debug("Leyendo archivo de instrucciones: {Ruta}", ruta);

        if (!File.Exists(ruta))
        {
            _logger.LogWarning("No se encontró el archivo de texto: {Ruta}", ruta);
            _log.Warning("No se encontró el archivo de texto: {Ruta}", ruta);
            return string.Empty;
        }

        var contenido = await File.ReadAllTextAsync(ruta, ct);
        _log.Debug("Archivo de instrucciones leído correctamente: {Ruta}", ruta);
        return contenido;
    }
}