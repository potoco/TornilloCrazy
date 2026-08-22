    using DocumentFormat.OpenXml.Office2013.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TornilloWeb.Servicio;
using DataServicio.Servicio;

namespace TornilloWeb.Pages;

public class IndexModel : PageModel
{


    private readonly ProveedorService _proveedorService;

    public IndexModel(ProveedorService proveedorService)
    {
        _proveedorService = proveedorService;
    }

    public string Mensaje { get; private set; } = string.Empty;
    public record FormularioRequest(string Buscar);
    // El atributo [Form] enlaza automáticamente todo el cuerpo del formulario
    public async Task OnPost([FromForm] FormularioRequest datos)
    {
        if (!ModelState.IsValid)
        {
            Mensaje = "Error en los datos provistos.";
            return;
        }

        var resultadoEmbedding = await ClienteWebPotoco.CrearVectorBusquedaUsuarioAsync(datos.Buscar);
        var reultadoCandidatos = await _proveedorService.BuscarProductoProveedor(Mensaje, resultadoEmbedding);

        var resultadosEspacial = reultadoCandidatos.Where(x => x.Distancia < 0.5).ToList();
        var resultadosSemejantes = reultadoCandidatos.Where(x => x.Distancia >= 0.5).ToList();

        // Acceso directo a las propiedades usando C# moderno
        Mensaje = $"¡Buscando.... {datos.Buscar}.";
    }
}
