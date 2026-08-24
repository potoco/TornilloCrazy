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

    public override string ToString() =>
        $"Codigo: {string.Join(" | ", Codigo ?? [])}, Descripcion: {string.Join(" | ", Descripcion ?? [])}, Precio: {string.Join(" | ", Precio ?? [])}, Rubro: {Rubro}";
}

public static class BuildFromExcel
{
    private static readonly string[] CodigoKeys = ["codigo", "articulo", "#", "sku", "cod"];
    private static readonly string[] DescripcionKeys = ["descripcion", "producto", "titulo", "nombre", "detalle"];
    private static readonly string[] PrecioKeys = ["precio", "total", "sub total", "neto", "bruto", "$", "lista"];
    private static readonly string[] RubroKeys = ["rubro", "categoria", "familia", "linea"];

    private sealed class HeaderDetectionResult
    {
        public int HeaderRowNumber { get; init; }
        public int MaxColumnIndex { get; init; }
        public int? CodigoColumn { get; init; }
        public int? DescripcionColumn { get; init; }
        public int? PrecioColumn { get; init; }
        public int? RubroColumn { get; init; }
        public List<int> AllPrecioColumns { get; init; } = [];
        public List<int> AllDescripcionColumns { get; init; } = [];
        public List<int> AllCodigoColumns { get; init; } = [];
    }

    public static List<ProductoFromExcelDto> ConstruirItemsDesdeExcel(string filePath)
    {
        var items = new List<ProductoFromExcelDto>();

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
            if (header is null || !header.DescripcionColumn.HasValue || !header.PrecioColumn.HasValue)
            {
                MessageBox.Show("No se encontró cabecera válida (Descripción y Precio) entre las filas 1 y 15.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return items;
            }

            for (var rowNumber = header.HeaderRowNumber + 1; rowNumber <= lastRow; rowNumber++)
            {
                var row = worksheet.Row(rowNumber);

                // 1. DESCRIPCIONES (La principal detectada va en el índice [0])
                var mainDescCell = row.Cell(header.DescripcionColumn.Value);
                var mainDesc = ObtenerTextoCelda(mainDescCell);
                if (string.IsNullOrWhiteSpace(mainDesc))
                {
                    continue; // Si no tiene descripción base, no es un ítem válido
                }

                var listaDescripciones = new List<ProductoFromExcelDto.DescripcionExcelDto>
                {
                    new()
                    {
                        Descripcion = mainDesc,
                        CeldaExcel = mainDescCell.Address.ToString()
                    }
                };

                // Otras columnas que coincidan como descripción
                foreach (var colIndex in header.AllDescripcionColumns.Where(c => c != header.DescripcionColumn.Value))
                {
                    var extraCell = row.Cell(colIndex);
                    var extraDesc = ObtenerTextoCelda(extraCell);
                    if (!string.IsNullOrWhiteSpace(extraDesc) && extraDesc != mainDesc)
                    {
                        listaDescripciones.Add(new ProductoFromExcelDto.DescripcionExcelDto
                        {
                            Descripcion = extraDesc,
                            CeldaExcel = extraCell.Address.ToString()
                        });
                    }
                }

                // 2. PRECIOS (El principal detectado va en el índice [0])
                var mainPriceCell = row.Cell(header.PrecioColumn.Value);
                var mainPriceText = ObtenerTextoCelda(mainPriceCell);
                if (!TryParseDecimalFlexible(mainPriceText, out _))
                {
                    continue; // Debe tener al menos el precio principal
                }

                var listaPrecios = new List<ProductoFromExcelDto.PrecioExcelDto>
                {
                    new()
                    {
                        Precio = mainPriceText,
                        CeldaExcel = mainPriceCell.Address.ToString()
                    }
                };

                // Otras columnas de precio/moneda encontradas en la cabecera
                foreach (var colIndex in header.AllPrecioColumns.Where(c => c != header.PrecioColumn.Value))
                {
                    var extraCell = row.Cell(colIndex);
                    var extraPriceText = ObtenerTextoCelda(extraCell);
                    if (TryParseDecimalFlexible(extraPriceText, out _))
                    {
                        listaPrecios.Add(new ProductoFromExcelDto.PrecioExcelDto
                        {
                            Precio = extraPriceText,
                            CeldaExcel = extraCell.Address.ToString()
                        });
                    }
                }

                // 3. CÓDIGOS (El principal en [0], luego adicionales si los hubiera)
                var listaCodigos = new List<ProductoFromExcelDto.CodigoExcelDto>();

                if (header.CodigoColumn.HasValue)
                {
                    var mainCodeCell = row.Cell(header.CodigoColumn.Value);
                    var mainCode = ObtenerTextoCelda(mainCodeCell);
                    if (!string.IsNullOrWhiteSpace(mainCode))
                    {
                        listaCodigos.Add(new ProductoFromExcelDto.CodigoExcelDto
                        {
                            Codigo = mainCode,
                            CeldaExcel = mainCodeCell.Address.ToString()
                        });
                    }
                }

                foreach (var colIndex in header.AllCodigoColumns.Where(c => c != header.CodigoColumn))
                {
                    var extraCell = row.Cell(colIndex);
                    var extraCode = ObtenerTextoCelda(extraCell);
                    if (!string.IsNullOrWhiteSpace(extraCode) && listaCodigos.All(c => c.Codigo != extraCode))
                    {
                        listaCodigos.Add(new ProductoFromExcelDto.CodigoExcelDto
                        {
                            Codigo = extraCode,
                            CeldaExcel = extraCell.Address.ToString()
                        });
                    }
                }

                // 4. RUBRO
                var rubro = header.RubroColumn.HasValue
                    ? ObtenerTextoCelda(row.Cell(header.RubroColumn.Value))
                    : string.Empty;

                // Crear objeto de salida
                items.Add(new ProductoFromExcelDto
                {
                    Codigo = listaCodigos.Count > 0 ? listaCodigos : null,
                    Descripcion = listaDescripciones,
                    Precio = listaPrecios,
                    Rubro = rubro
                });
            }

            return items;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo leer el archivo Excel: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return new List<ProductoFromExcelDto>();
        }
    }

    private static HeaderDetectionResult? DetectarCabecera(IXLWorksheet worksheet, int maxColumnIndex)
    {
        var maxHeaderRow = 15;

        for (var rowNumber = 1; rowNumber <= maxHeaderRow; rowNumber++)
        {
            var row = worksheet.Row(rowNumber);

            var (codigoCol, allCodigos) = FindColumns(row, maxColumnIndex, CodigoKeys);
            var (descCol, allDescs) = FindColumns(row, maxColumnIndex, DescripcionKeys);
            var (precioCol, allPrecios) = FindColumns(row, maxColumnIndex, PrecioKeys);
            var (rubroCol, _) = FindColumns(row, maxColumnIndex, RubroKeys);

            if (descCol.HasValue && precioCol.HasValue)
            {
                return new HeaderDetectionResult
                {
                    HeaderRowNumber = rowNumber,
                    MaxColumnIndex = maxColumnIndex,
                    CodigoColumn = codigoCol,
                    DescripcionColumn = descCol,
                    PrecioColumn = precioCol,
                    RubroColumn = rubroCol,
                    AllCodigoColumns = allCodigos,
                    AllDescripcionColumns = allDescs,
                    AllPrecioColumns = allPrecios
                };
            }
        }

        return null;
    }

    private static (int? BestColumn, List<int> AllMatchedColumns) FindColumns(IXLRow row, int maxColumnIndex, string[] keywords)
    {
        var bestScore = int.MinValue;
        int? bestColumn = null;
        var matchedColumns = new List<int>();

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

                if (score > 0)
                {
                    if (!matchedColumns.Contains(col))
                    {
                        matchedColumns.Add(col);
                    }

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestColumn = col;
                    }
                }
            }
        }

        return (bestScore > 0 ? bestColumn : null, matchedColumns);
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

        var text = input.Trim().Replace(" ", "").Replace("$", "");

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