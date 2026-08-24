using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using TornilloWeb.Servicio;

namespace TornilloWeb.Pages
{
    public class CargarExcelModel : PageModel
    {
        public sealed class PaginacionExcelModel
        {
            public string? LoteId { get; init; }
            public int CurrentPage { get; init; }
            public int TotalPages { get; init; }
            public string? FiltroDescripcion { get; init; }
            public IEnumerable<int> PaginasVisibles { get; init; } = [];
            public int DisplayCurrentPage => TotalPages == 0 ? 0 : CurrentPage;
            public bool HasPrevious => CurrentPage > 1;
            public bool HasNext => CurrentPage < TotalPages;
            public bool MostrarNavegacion => TotalPages > 1;
        }

        private sealed class ResultadoExcelCache
        {
            public List<ProductoFromExcelDto> Items { get; init; } = [];
            public string? NombreOriginalArchivo { get; init; }
            public string? NombreArchivoGuardado { get; init; }
        }

        private const int PageSize = 50;
        private readonly IWebHostEnvironment _environment;
        private readonly IMemoryCache _memoryCache;

        public CargarExcelModel(IWebHostEnvironment environment, IMemoryCache memoryCache)
        {
            _environment = environment;
            _memoryCache = memoryCache;
        }

        [BindProperty]
        public IFormFile? ArchivoExcel { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? LoteId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public string? FiltroDescripcion { get; set; }

        public List<ProductoFromExcelDto> Resultados { get; private set; } = [];
        public string? MensajeError { get; private set; }
        public string? MensajeExito { get; private set; }
        public string? NombreOriginalArchivo { get; private set; }
        public string? NombreArchivoGuardado { get; private set; }
        public int TotalItems { get; private set; }
        public int TotalPages { get; private set; }
        public bool TieneResultadosProcesados { get; private set; }

        public int CurrentPage => PageNumber < 1 ? 1 : PageNumber;
        public bool TienePaginacion => TotalPages > 1;
        public int StartRowNumber => TotalItems == 0 ? 0 : ((CurrentPage - 1) * PageSize) + 1;

        public IEnumerable<int> PaginasVisibles
        {
            get
            {
                if (TotalPages == 0)
                {
                    return [];
                }

                var inicio = Math.Max(1, CurrentPage - 2);
                var fin = Math.Min(TotalPages, CurrentPage + 2);

                return Enumerable.Range(inicio, (fin - inicio) + 1);
            }
        }

        public PaginacionExcelModel Paginacion => new()
        {
            LoteId = LoteId,
            CurrentPage = CurrentPage,
            TotalPages = TotalPages,
            FiltroDescripcion = FiltroDescripcion,
            PaginasVisibles = PaginasVisibles
        };

        public void OnGet()
        {
            CargarDesdeCache();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ArchivoExcel is null || ArchivoExcel.Length == 0)
            {
                MensajeError = "Debe seleccionar un archivo Excel.";
                return Page();
            }

            var extension = Path.GetExtension(ArchivoExcel.FileName);
            if (!string.Equals(extension, ".xlsx", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(extension, ".xls", StringComparison.OrdinalIgnoreCase))
            {
                MensajeError = "El archivo debe ser un Excel válido (.xlsx o .xls).";
                return Page();
            }

            var targetDirectory = Path.Combine(_environment.ContentRootPath, "AppData", "tmp", "lprecios");
            Directory.CreateDirectory(targetDirectory);

            NombreOriginalArchivo = Path.GetFileName(ArchivoExcel.FileName);
            var baseName = Path.GetFileNameWithoutExtension(NombreOriginalArchivo);
            var safeBaseName = string.Join("_", baseName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
            var uniqueSuffix = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            NombreArchivoGuardado = $"{safeBaseName}_{uniqueSuffix}{extension}";
            var fullPath = Path.Combine(targetDirectory, NombreArchivoGuardado);

            await using (var stream = System.IO.File.Create(fullPath))
            {
                await ArchivoExcel.CopyToAsync(stream);
            }

            var respuesta = BuildFromExcel.ExtraerData(fullPath);

            if (!respuesta.Exito)
            {
                MensajeError = string.IsNullOrWhiteSpace(respuesta.MensajeError)
                    ? "No se pudo procesar el archivo Excel."
                    : respuesta.MensajeError;
                return Page();
            }

            var todosLosResultados = respuesta.Salida ?? [];

            LoteId = Guid.NewGuid().ToString("N");
            PageNumber = 1;
            FiltroDescripcion = null;

            _memoryCache.Set(LoteId, new ResultadoExcelCache
            {
                Items = todosLosResultados,
                NombreOriginalArchivo = NombreOriginalArchivo,
                NombreArchivoGuardado = NombreArchivoGuardado
            }, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(30)
            });

            TieneResultadosProcesados = true;
            AplicarPaginacion(todosLosResultados);
            MensajeExito = $"Archivo cargado y procesado: {NombreArchivoGuardado}";
            return Page();
        }

        private void CargarDesdeCache()
        {
            if (string.IsNullOrWhiteSpace(LoteId))
            {
                return;
            }

            if (!_memoryCache.TryGetValue<ResultadoExcelCache>(LoteId, out var cache) || cache is null)
            {
                MensajeError = "La sesión de resultados expiró. Vuelva a cargar el archivo Excel.";
                return;
            }

            NombreOriginalArchivo = cache.NombreOriginalArchivo;
            NombreArchivoGuardado = cache.NombreArchivoGuardado;
            MensajeExito = string.IsNullOrWhiteSpace(NombreArchivoGuardado)
                ? "Archivo cargado y procesado."
                : $"Archivo cargado y procesado: {NombreArchivoGuardado}";

            TieneResultadosProcesados = true;
            var resultadosFiltrados = AplicarFiltroDescripcion(cache.Items);
            AplicarPaginacion(resultadosFiltrados);
        }

        private List<ProductoFromExcelDto> AplicarFiltroDescripcion(List<ProductoFromExcelDto> items)
        {
            var filtro = (FiltroDescripcion ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(filtro))
            {
                return items;
            }

            return items
                .Where(item => (item.Descripcion ?? [])
                    .Any(d => !string.IsNullOrWhiteSpace(d.Descripcion)
                        && d.Descripcion!.Contains(filtro, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        private void AplicarPaginacion(List<ProductoFromExcelDto> todos)
        {
            TotalItems = todos.Count;
            TotalPages = TotalItems == 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);

            if (TotalPages == 0)
            {
                PageNumber = 1;
                Resultados = [];
                return;
            }

            if (PageNumber < 1)
            {
                PageNumber = 1;
            }
            else if (PageNumber > TotalPages)
            {
                PageNumber = TotalPages;
            }

            Resultados = todos
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }
    }
}
