using DataServicio.Servicio;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace TornilloWeb.Controller;


[ApiController]
[Route("api/proveedor")]
[Route("api/proveedores")]
public class ProveedorController : ControllerBase
{
    private readonly ProveedorService _proveedorService;

    public ProveedorController(ProveedorService proveedorService)
    {
        _proveedorService = proveedorService;
    }

    [HttpPost]
    public async Task<ActionResult<ProveedorDto>> CrearProveedor([FromBody] CrearProveedorRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

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
        {
            return NotFound(new { mensaje = "Proveedor no encontrado" });
        }

        return Ok(proveedor);
    }

    public sealed class CrearProveedorRequest
    {
        [Required]
        [StringLength(30)]
        public string CUIT { get; set; } = string.Empty;

        [Required]
        [StringLength(130)]
        public string RazonSocial { get; set; } = string.Empty;
    }
}
