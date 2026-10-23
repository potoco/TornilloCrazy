using DataServicio;
using DataServicio.Servicio;
using DataServicio.Tabla;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace TornilloWeb.Pages.Admin.Productos
{
    public class ProductoEditModel : PageModel
    {
        public sealed class ProductoForm
        {
            [Required, StringLength(200)]
            public string Nombre { get; set; } = string.Empty;

            [StringLength(240)]
            public string Rubros { get; set; } = string.Empty;

            [StringLength(240)]
            public string Sinonimos { get; set; } = string.Empty;

            [StringLength(30)]
            public string? Codigo { get; set; }

            [Range(0, int.MaxValue)]
            public int Cantidad { get; set; }

            [Range(0, 999999999)]
            public decimal PrecioUnitario { get; set; }

            public string? DescripcionDetallada { get; set; }
        }

        private static readonly HashSet<string> ExtensionesImagenPermitidas = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

        private readonly FerreteriaDbContext _context;
        private readonly ProductoService _productoService;
        private readonly IWebHostEnvironment _environment;

        public ProductoEditModel(FerreteriaDbContext context, ProductoService productoService, IWebHostEnvironment environment)
        {
            _context = context;
            _productoService = productoService;
            _environment = environment;
        }

        [BindProperty(SupportsGet = true)]
        public int? ProductoId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Buscar { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Tab { get; set; }

        [BindProperty]
        public ProductoForm ProductoInput { get; set; } = new();

        [BindProperty]
        public IFormFile? ImagenArchivo { get; set; }

        public List<ProductoTbl> Productos { get; private set; } = [];
        public ProductoTbl? ProductoSeleccionado { get; private set; }
        public List<ProductoImagenTbl> ImagenesProducto { get; private set; } = [];
        public bool EsEdicion => ProductoSeleccionado is not null;

        [TempData]
        public string? MensajeOk { get; set; }

        [TempData]
        public string? MensajeError { get; set; }

        public async Task OnGetAsync()
        {
            await CargarPaginaAsync(ProductoId);
        }

        public async Task<IActionResult> OnPostCrearAsync()
        {
            ModelState.Clear();
            if (!TryValidateModel(ProductoInput, nameof(ProductoInput)))
            {
                await CargarPaginaAsync(null);
                return Page();
            }

            var producto = new ProductoTbl
            {
                Nombre = ProductoInput.Nombre,
                Rubros = ProductoInput.Rubros,
                Sinonimos = ProductoInput.Sinonimos,
                Codigo = ProductoInput.Codigo,
                Cantidad = ProductoInput.Cantidad,
                PrecioUnitario = ProductoInput.PrecioUnitario,
                DescripcionDetallada = ProductoInput.DescripcionDetallada,
                Estado = 0
            };

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            MensajeOk = "Producto creado correctamente.";
            return RedirectToPage(new { productoId = producto.ProductoId });
        }

        public async Task<IActionResult> OnPostActualizarAsync(int productoId)
        {
            ModelState.Clear();
            if (!TryValidateModel(ProductoInput, nameof(ProductoInput)))
            {
                await CargarPaginaAsync(productoId);
                return Page();
            }

            var producto = await _context.Productos.FirstOrDefaultAsync(p => p.ProductoId == productoId);
            if (producto is null)
            {
                MensajeError = "El producto no existe.";
                return RedirectToPage();
            }

            producto.Nombre = ProductoInput.Nombre;
            producto.Rubros = ProductoInput.Rubros;
            producto.Sinonimos = ProductoInput.Sinonimos;
            producto.Codigo = ProductoInput.Codigo;
            producto.Cantidad = ProductoInput.Cantidad;
            producto.PrecioUnitario = ProductoInput.PrecioUnitario;
            producto.DescripcionDetallada = ProductoInput.DescripcionDetallada;

            await _context.SaveChangesAsync();

            MensajeOk = "Producto actualizado correctamente.";
            return RedirectToPage(new { productoId });
        }

        public async Task<IActionResult> OnPostEliminarProductoAsync(int productoId)
        {
            var producto = await _context.Productos.FirstOrDefaultAsync(p => p.ProductoId == productoId);
            if (producto is not null)
            {
                producto.Estado = -1;
                await _context.SaveChangesAsync();
                MensajeOk = "Producto desactivado correctamente.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAgregarImagenAsync(int productoId, bool esPrincipal)
        {
            if (ImagenArchivo is null || ImagenArchivo.Length == 0)
            {
                MensajeError = "Seleccione un archivo de imagen.";
                return RedirectToPage(new { productoId, tab = "imagenes" });
            }

            var extension = Path.GetExtension(ImagenArchivo.FileName);
            if (!ExtensionesImagenPermitidas.Contains(extension))
            {
                MensajeError = "Formato no permitido. Use .jpg, .jpeg, .png, .webp o .gif.";
                return RedirectToPage(new { productoId, tab = "imagenes" });
            }

            var nombreArchivo = $"{productoId}_{Guid.NewGuid():N}{extension}";
            var carpetaDestino = Path.Combine(_environment.WebRootPath, "img", "productos");
            Directory.CreateDirectory(carpetaDestino);

            var rutaCompleta = Path.Combine(carpetaDestino, nombreArchivo);
            await using (var stream = System.IO.File.Create(rutaCompleta))
            {
                await ImagenArchivo.CopyToAsync(stream);
            }

            await _productoService.AgregarImagenProductoAsync(productoId, nombreArchivo, esPrincipal);
            MensajeOk = "Imagen agregada correctamente.";
            return RedirectToPage(new { productoId, tab = "imagenes" });
        }

        public async Task<IActionResult> OnPostEliminarImagenAsync(int productoId, int productoImagenId)
        {
            var nombreArchivo = await _productoService.EliminarImagenProductoAsync(productoId, productoImagenId);
            if (nombreArchivo is null)
            {
                MensajeError = "La imagen no existe.";
                return RedirectToPage(new { productoId, tab = "imagenes" });
            }

            var rutaCompleta = Path.Combine(_environment.WebRootPath, "img", "productos", nombreArchivo);
            if (System.IO.File.Exists(rutaCompleta))
            {
                System.IO.File.Delete(rutaCompleta);
            }

            MensajeOk = "Imagen eliminada correctamente.";
            return RedirectToPage(new { productoId, tab = "imagenes" });
        }

        public async Task<IActionResult> OnPostMarcarPrincipalAsync(int productoId, int productoImagenId)
        {
            await _productoService.MarcarImagenPrincipalAsync(productoId, productoImagenId);
            MensajeOk = "Imagen principal actualizada.";
            return RedirectToPage(new { productoId, tab = "imagenes" });
        }

        private async Task CargarPaginaAsync(int? productoId)
        {
            Productos = string.IsNullOrWhiteSpace(Buscar)
                ? await _productoService.ListarUltimosProductosAsync(8)
                : await _productoService.BuscarProductosPorNombreAsync(Buscar);

            if (productoId.HasValue)
            {
                ProductoSeleccionado = await _context.Productos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.ProductoId == productoId.Value);

                if (ProductoSeleccionado is not null)
                {
                    ProductoInput = new ProductoForm
                    {
                        Nombre = ProductoSeleccionado.Nombre,
                        Rubros = ProductoSeleccionado.Rubros,
                        Sinonimos = ProductoSeleccionado.Sinonimos,
                        Codigo = ProductoSeleccionado.Codigo,
                        Cantidad = ProductoSeleccionado.Cantidad,
                        PrecioUnitario = ProductoSeleccionado.PrecioUnitario,
                        DescripcionDetallada = ProductoSeleccionado.DescripcionDetallada
                    };

                    ImagenesProducto = await _productoService.ObtenerImagenesProductoAsync(productoId.Value);
                }
            }
        }
    }
}
