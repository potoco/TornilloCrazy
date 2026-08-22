using DataServicio.Servicio;
using DataServicio.Modelo;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using TornilloWeb.Servicio;



namespace TornilloWeb.Controller;

[ApiController]
[Route("api/proveedor")]
[Route("api/proveedores")]
public class ProveedorController : ControllerBase
{
    private readonly ProveedorService _proveedorService;
    private readonly IWebHostEnvironment _environment;


    public ProveedorController(ProveedorService proveedorService, IWebHostEnvironment environment)
    {
        _proveedorService = proveedorService;
        _environment = environment;
    }

    [HttpPost]
    public async Task<ActionResult<ProveedorDto>> CrearProveedor([FromBody] CrearProveedorRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var response = await _proveedorService.CrearProveedorAsync(request.CUIT, request.RazonSocial);
        return CreatedAtAction(nameof(ObtenerProveedorPorId), new { proveedorId = response.ProveedorId }, response);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProveedorDto>>> ObtenerProveedores()
    {
        var proveedores = await _proveedorService.ObtenerProveedoresAsync();
        return Ok(proveedores);
    }

    [HttpGet("{proveedorId:int}")]
    public async Task<ActionResult<ProveedorDto>> ObtenerProveedorPorId(int proveedorId)
    {
        var proveedor = await _proveedorService.ObtenerProveedorPorIdAsync(proveedorId);
        if (proveedor is null)
            return NotFound(new { mensaje = "Proveedor no encontrado" });

        return Ok(proveedor);
    }

    [HttpPost("lista-precio")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ListaPrecioArchivoDto>> SubirListaPrecio([FromForm] SubirListaPrecioRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        // 1. Validaciones del archivo
        if (request.ArchivoExcel == null || request.ArchivoExcel.Length == 0)
            return BadRequest(new { mensaje = "El archivo excel es obligatorio y no puede estar vacío." });

        var extension = Path.GetExtension(request.ArchivoExcel.FileName);
        if (!string.Equals(extension, ".xlsx", StringComparison.OrdinalIgnoreCase) && !string.Equals(extension, ".xls", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { mensaje = "Solo se permite archivo .xlsx o .xls" });

        // 2. Preparar nombres y rutas físicas
        var nombreArchivoOriginal = Path.GetFileName(request.ArchivoExcel.FileName);
        var nombreArchivoGuid = $"{Guid.NewGuid():N}{extension}";

        var carpetaDestino = Path.Combine(_environment.ContentRootPath,"AppData","Proveedores",$"Proveedor-{request.ProveedorId}");
        Directory.CreateDirectory(carpetaDestino);
        var rutaDestino = Path.Combine(carpetaDestino, nombreArchivoGuid);

        try
        {
            // 3. Guardar archivo físico en disco
            await using (var fileStream = new FileStream(rutaDestino, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await request.ArchivoExcel.CopyToAsync(fileStream);
            }

            // 4. Guardar registro en BD llamando al servicio compartido
            var responseListp = await _proveedorService.RegistrarListaPrecioAsync(request.ProveedorId,nombreArchivoGuid,nombreArchivoOriginal);

            return Ok(responseListp);
        }
        catch (KeyNotFoundException ex)
        {
            // Si no existe el proveedor o falla la BD, borramos el archivo físico recién creado
            if (System.IO.File.Exists(rutaDestino))
                System.IO.File.Delete(rutaDestino);

            return NotFound(new { mensaje = ex.Message });
        }
        catch (Exception)
        {
            // Limpieza ante cualquier otro error durante el guardado
            if (System.IO.File.Exists(rutaDestino))
                System.IO.File.Delete(rutaDestino);

            throw;
        }
    }



    public sealed class SubirListaPrecioRequest
    {
        [Required(ErrorMessage = "El ID del proveedor es obligatorio.")]
        public int ProveedorId { get; set; }

        [Required(ErrorMessage = "Debe adjuntar un archivo Excel.")]
        public IFormFile ArchivoExcel { get; set; } = null!;
    }


    public sealed class CrearProveedorRequest
    {
        [Required, StringLength(19)]
        public string CUIT { get; set; } = string.Empty;

        [Required, StringLength(130)]
        public string RazonSocial { get; set; } = string.Empty;
    }

}
