using DataServicio.Modelo;
using DataServicio.Servicio;
using DocumentFormat.OpenXml.Office2013.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog;
using System.Globalization;
using System.Text;
using TornilloWeb.Servicio;

namespace TornilloWeb.Pages.Admin;

public class IndexModel : PageModel
{
    private readonly ProductoService _productoService;
    private readonly ClienteWebPotoco _clienteWebPotoco;
    private readonly ProveedorService _proveedorService;
    private readonly InstruccionesTextoHelper _instruccionesTextoHelper;
    private readonly Serilog.ILogger _log;

    public IndexModel(ProveedorService proveedorService, ProductoService productoService, InstruccionesTextoHelper instruccionesTextoHelper, ClienteWebPotoco clienteWebPotoco)
    {
        _proveedorService = proveedorService;
        _productoService = productoService;
        _instruccionesTextoHelper = instruccionesTextoHelper;
        _clienteWebPotoco = clienteWebPotoco;
        _log = Log.ForContext<IndexModel>();
        _log.Debug("IndexModel inicializado.");
    }

    public string Mensaje { get; private set; } = string.Empty;
    public IReadOnlyList<ProductoResultadoDto> Resultados { get; private set; } = [];
    public record FormularioRequest(string Buscar);


    public async Task OnGetAsync()
    {
        _log.Information("Ejecución de OnGetAsync en IndexModel.");
        await Task.CompletedTask;
    }

    public async Task OnPost([FromForm] FormularioRequest datos)
    {
        _log.Information("Inicio de búsqueda en OnPost. Texto buscado: {Buscar}", datos.Buscar);
        if (!ModelState.IsValid)
        {
            _log.Warning("ModelState inválido en OnPost.");
            Mensaje = "Error en los datos provistos.";
            return;
        }

        List<ProductoResultadoDto> acumulado = new();

        var instrucciones = await _instruccionesTextoHelper.ObtenerIntruccionEmbBusUsuarioAsync();
        instrucciones += $"{datos.Buscar}.";


        // busqueda VECTORIAL
        var resultadoEmbedding = await _clienteWebPotoco.CrearVectorBusquedaUsuarioAsync(instrucciones);
        var productosEncontrados = await _productoService.ObtenerProductoPorVectorAsync(resultadoEmbedding);
        if(productosEncontrados != null)
        {
            acumulado.AddRange(productosEncontrados);
            _log.Information("Resultados por búsqueda vectorial: {Cantidad}", productosEncontrados.Count);
        }

        // busqueda TRADICIONAL
        var soloImportante = SoloPalabrasImportante(datos.Buscar);
        _log.Debug("Palabras clave relevantes detectadas: {Cantidad}", soloImportante.Length);
        foreach (var palabra in soloImportante) {
            var resultadoProveedores = await _productoService.ObtenerProductoPorTextoAsync(palabra);
            if(resultadoProveedores != null)
            {
                acumulado.AddRange(resultadoProveedores);
                _log.Debug("Resultados por palabra '{Palabra}': {Cantidad}", palabra, resultadoProveedores.Count);
            }
        }

        // Mostrar resultados ELIMINANDO LOS DUPLICADOS 
        acumulado = acumulado
                    .DistinctBy(p => p.ProductoId)
                    .OrderBy(p=>p.Distancia)
                    .ToList();

        Resultados = acumulado;


        // Acceso directo a las propiedades usando C# moderno
        Mensaje = $"¡Buscando.... {datos.Buscar}. Se encontraron {Resultados.Count} resultados.";
        _log.Information("Fin de OnPost. Resultados finales: {Cantidad}", Resultados.Count);
    }
    public string ObtenerPrimerPrecio(string lineaCruda)
    {
        _log.Debug("ObtenerPrimerPrecio invocado.");
        if (string.IsNullOrWhiteSpace(lineaCruda))
        {
            _log.Warning("ObtenerPrimerPrecio recibió una línea vacía.");
            return "N/D";
        }

        //var primerValor = lineaCruda
        //    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        //    .FirstOrDefault();
        var valores = lineaCruda.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var primerValor = valores.LastOrDefault();
        if(primerValor == null)
        {
            _log.Warning("No se encontró ningún valor de precio en la línea recibida.");
            return "N/D";
        }
        primerValor = primerValor.Replace("$", "").Replace("USD", "").Replace("usd", "").Replace(",", ".").Trim();
        bool existe = decimal.TryParse(primerValor, NumberStyles.Currency, CultureInfo.InvariantCulture, out decimal precioDecimal);
        _log.Debug("Resultado parseo de precio: {Existe}", existe);
        return existe ? $"$ {precioDecimal.ToString("###,###,###,###.00")}" : "N/D";
    }
    private string[] SoloPalabrasImportante(string frase)
    {
        _log.Debug("SoloPalabrasImportante invocado para frase de longitud {Longitud}.", frase?.Length ?? 0);

        string[] stopwords = {
            "el","la","los","las","un","una","unos","unas",
            "de","del","al","para","por","con","sin","sobre",
            "entre","hasta","desde","tras","durante",
            "y","o","u","se","que","como",
            "x","x1","x2","x3","x4","x5","x6","x10","x12","x20",
            "unidad","unidades","pack","paquete",
            "tipo","tamaño","medida","varios","varias",
            "nuevo","nueva","nuevos","nuevas",
            "oferta","promo","promoción","rebaja"
        };

        string[] keywords = frase
            .ToLower().Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .Aggregate("", (a, b) => a + b).Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(p => !stopwords.Contains(p)).ToArray();

        _log.Debug("SoloPalabrasImportante devolvió {Cantidad} palabras.", keywords.Length);
        return keywords;

    }
}
