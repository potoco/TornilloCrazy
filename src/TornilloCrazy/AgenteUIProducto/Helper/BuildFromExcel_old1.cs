using ClosedXML.Excel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
namespace AgenteUIProducto.Helper;
public sealed class ProductoFromExcelDto
{
    public class CodigoExcelDto
    {
        public string? Codigo { get; set; }
        public string CeldaExcel { get; set; } = string.Empty;
        public override string ToString() => $"{Codigo}";
    }
    public class DescripcionExcelDto
    {
        public string? Descripcion { get; set; }
        public string CeldaExcel { get; set; } = string.Empty;
        public override string ToString() => $"{Descripcion}";
    }
    public class PrecioExcelDto
    {
        public string? Precio { get; set; }
        public string CeldaExcel { get; set; } = string.Empty;
        public override string ToString() => $"{Precio}";
    }
    public List<CodigoExcelDto>? Codigo { get; set; } 
    public List<DescripcionExcelDto>? Descripcion { get; set; } 
    public List<PrecioExcelDto>? Precio { get; set; } 
    public string Rubro { get; set; } = string.Empty;
    public override string ToString() => $"Codigo: {Codigo}, Descripcion: {Descripcion}, Precio: {Precio}, Rubro: {Rubro}";
}
public sealed class PrecioItemDto
{
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string Rubro { get; set; } = string.Empty;
    public override string ToString() => $"Codigo: {Codigo}, Descripcion: {Descripcion}, Precio: {Precio}, Rubro: {Rubro}";
    public string ToString2() => $"{Descripcion};{Precio}";
}
public static class BuildFromExcel_old1
{
    private static readonly string[] CodigoKeys = ["codigo", "articulo", "#"];
    private static readonly string[] DescripcionKeys = ["descripcion", "producto", "titulo", "nombre"];
    private static readonly string[] PrecioKeys = ["precio", "total", "sub total", "neto", "bruto", "$"];
    private static readonly string[] RubroKeys = ["rubro"];

    private sealed class HeaderDetectionResult
    {
        public int HeaderRowNumber { get; init; }
        public int MaxColumnIndex { get; init; }
        public int? CodigoColumn { get; init; }
        public int? DescripcionColumn { get; init; }
        public int? PrecioColumn { get; init; }
        public int? RubroColumn { get; init; }
    }

    public static List<PrecioItemDto> ConstruirItemsDesdeExcel(string filePath)
    {
        var items = new List<PrecioItemDto>();

        try
        {
            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet is null)
            {
                return items;
            }

            var usedRange = worksheet.RangeUsed(XLCellsUsedOptions.AllContents);
            if (usedRange is null)
            {
                return items;
            }

            var maxColumnIndex = usedRange.RangeAddress.LastAddress.ColumnNumber;
            var lastRow = usedRange.RangeAddress.LastAddress.RowNumber;

            var header = DetectarCabecera(worksheet, maxColumnIndex);
            if (header is null)
            {
                MessageBox.Show("No se encontró cabecera válida (Descripción y Precio) entre las filas 1 y 15.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return items;
            }

            for (var rowNumber = header.HeaderRowNumber + 1; rowNumber <= lastRow; rowNumber++)
            {
                var row = worksheet.Row(rowNumber);

                var descripcion = ObtenerTextoCelda(row.Cell(header.DescripcionColumn!.Value));
                if (string.IsNullOrWhiteSpace(descripcion))
                {
                    continue;
                }

                var precioText = ObtenerTextoCelda(row.Cell(header.PrecioColumn!.Value));
                if (!TryParseDecimalFlexible(precioText, out var precio))
                {
                    continue;
                }

                var codigo = header.CodigoColumn.HasValue
                    ? ObtenerTextoCelda(row.Cell(header.CodigoColumn.Value))
                    : string.Empty;

                var rubro = header.RubroColumn.HasValue
                    ? ObtenerTextoCelda(row.Cell(header.RubroColumn.Value))
                    : string.Empty;

                items.Add(new PrecioItemDto
                {
                    Codigo = codigo,
                    Descripcion = descripcion,
                    Precio = precio,
                    Rubro = rubro
                });
            }

            return items;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo leer el archivo Excel: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return new List<PrecioItemDto>();
        }
    }

    private static HeaderDetectionResult? DetectarCabecera(IXLWorksheet worksheet, int maxColumnIndex)
    {
        var maxHeaderRow = 15;

        for (var rowNumber = 1; rowNumber <= maxHeaderRow; rowNumber++)
        {
            var row = worksheet.Row(rowNumber);

            var codigoColumn = FindBestColumn(row, maxColumnIndex, CodigoKeys);
            var descripcionColumn = FindBestColumn(row, maxColumnIndex, DescripcionKeys);
            var precioColumn = FindBestColumn(row, maxColumnIndex, PrecioKeys);
            var rubroColumn = FindBestColumn(row, maxColumnIndex, RubroKeys);

            if (descripcionColumn.HasValue && precioColumn.HasValue)
            {
                return new HeaderDetectionResult
                {
                    HeaderRowNumber = rowNumber,
                    MaxColumnIndex = maxColumnIndex,
                    CodigoColumn = codigoColumn,
                    DescripcionColumn = descripcionColumn,
                    PrecioColumn = precioColumn,
                    RubroColumn = rubroColumn
                };
            }
        }

        return null;
    }

    private static int? FindBestColumn(IXLRow row, int maxColumnIndex, string[] keywords)
    {
        var bestScore = int.MinValue;
        int? bestColumn = null;

        for (var col = 1; col <= maxColumnIndex; col++)
        {
            var raw = row.Cell(col).GetString();
            var normalized = Normalize(raw);

            if (string.IsNullOrWhiteSpace(normalized))
            {
                continue;
            }

            for (var i = 0; i < keywords.Length; i++)
            {
                var key = Normalize(keywords[i]);
                var score = MatchScore(normalized, key, i);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestColumn = col;
                }
            }
        }

        return bestScore > 0 ? bestColumn : null;
    }

    private static int MatchScore(string text, string keyword, int priorityIndex)
    {
        var priorityBoost = (100 - priorityIndex * 10);

        if (text == keyword)
        {
            return 1000 + priorityBoost;
        }

        if (Regex.IsMatch(text, $@"\b{Regex.Escape(keyword)}\b", RegexOptions.CultureInvariant))
        {
            return 700 + priorityBoost;
        }

        if (text.Contains(keyword, StringComparison.Ordinal))
        {
            return 500 + priorityBoost;
        }

        return 0;
    }

    private static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var lower = text.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(lower.Length);

        foreach (var c in lower)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    private static string ObtenerTextoCelda(IXLCell cell)
    {
        if (cell.IsEmpty())
        {
            return string.Empty;
        }

        var text = cell.GetFormattedString()?.Trim() ?? string.Empty;
        text = text.Replace("\r", "").Replace("\n", " ");
        return text;
    }

    private static bool TryParseDecimalFlexible(string input, out decimal value)
    {
        value = 0m;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var text = input.Trim().Replace(" ", "");

        if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.GetCultureInfo("es-ES"), out value))
        {
            return true;
        }

        if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
        {
            return true;
        }

        var normalized = text.Replace(".", "").Replace(",", ".");
        return decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }
}