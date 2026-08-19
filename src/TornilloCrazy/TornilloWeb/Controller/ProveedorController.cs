using Microsoft.AspNetCore.Mvc;

namespace TornilloWeb.Controller;


[ApiController]
[Route("api/[controller]")]
public class ProveedorController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { mensaje = "API funcionando" });
    }
}
