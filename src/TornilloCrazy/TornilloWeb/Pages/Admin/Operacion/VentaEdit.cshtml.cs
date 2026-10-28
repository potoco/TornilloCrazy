using DataServicio;
using DataServicio.Servicio;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace TornilloWeb.Pages.Admin.Operacion;

public class VentaEditModel(FerreteriaDbContext context, VentaService ventas, ILogger<VentaEditModel> logger) : PageModel
{
    [BindProperty, Range(1, int.MaxValue)] public int ClienteId { get; set; }
    [BindProperty, Range(0, 100)] public decimal IvaPorcentaje { get; set; } = 21;
    [BindProperty, StringLength(30000)] public string CarritoJson { get; set; } = "[]";
    [TempData] public string? MensajeOk { get; set; }
    [TempData] public bool VentaIngresada { get; set; }
    public List<ClienteOpcion> Clientes { get; private set; } = [];
    public sealed record ClienteOpcion(int Id, string Nombre, decimal DescuentoPorcentaje);
    public async Task OnGetAsync() => await CargarClientesAsync();

    public async Task<IActionResult> OnGetProductosAsync(string? buscar, int pagina = 1)
    {
        pagina = Math.Clamp(pagina, 1, 100000);
        var query = context.Productos.AsNoTracking().Where(p => p.Estado >= 0);
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim();
            query = query.Where(p => p.Nombre.Contains(texto) || (p.Codigo != null && p.Codigo.Contains(texto)));
        }
        var total = await query.CountAsync();
        var productos = await query.OrderBy(p => p.Nombre).ThenBy(p => p.ProductoId)
            .Skip((pagina - 1) * 10).Take(10)
            .Select(p => new
            {
                id = p.ProductoId, nombre = p.Nombre, codigo = p.Codigo, stock = p.Cantidad,
                precio = p.PrecioUnitario, rubros = p.Rubros, sinonimos = p.Sinonimos,
                descripcion = p.DescripcionDetallada,
                imagen = context.ProductoImagenes.Where(i => i.ProductoId == p.ProductoId)
                    .OrderByDescending(i => i.EsPrincipal).ThenByDescending(i => i.ProductoImagenId)
                    .Select(i => i.NombreArchivo).FirstOrDefault()
            }).ToListAsync();
        return new JsonResult(new { productos, total, pagina });
    }

    public async Task<IActionResult> OnPostIngresarAsync()
    {
        if (ModelState.IsValid)
        {
            try
            {
                var lineas = JsonSerializer.Deserialize<List<VentaService.Linea>>(CarritoJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                await ventas.IngresarAsync(ClienteId, IvaPorcentaje, lineas ?? []);
                VentaIngresada = true;
                MensajeOk = "Venta ingresada correctamente. Stock actualizado.";
                return RedirectToPage();
            }
            catch (JsonException) { ModelState.AddModelError(string.Empty, "El carrito no es válido."); }
            catch (InvalidOperationException ex) { ModelState.AddModelError(string.Empty, ex.Message); }
            catch (Exception ex)
            {
                logger.LogError(ex, "No se pudo ingresar la venta");
                ModelState.AddModelError(string.Empty, "No se pudo ingresar la venta. No se guardaron cambios; intente nuevamente.");
            }
        }
        await CargarClientesAsync();
        return Page();
    }

    private async Task CargarClientesAsync() => Clientes = await context.Clientes.AsNoTracking()
        .Where(c => c.Estado >= 0).OrderBy(c => c.Persona.Nombre)
        .Select(c => new ClienteOpcion(c.ClienteId, c.Persona.Nombre + " " + c.Persona.ApePaterno, c.DescuentoPorcentaje))
        .ToListAsync();
}
