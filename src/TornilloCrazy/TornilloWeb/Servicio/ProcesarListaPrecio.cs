using ClosedXML.Excel;
using DataServicio.Servicio;
using System.Data;
using System.Text;

namespace TornilloWeb.Servicio;

public static class ProcesarListaPrecio
{
    private static ListaPrecioArchivoDto? _responseListp;
    public static async Task Procesar(string filePath, ListaPrecioArchivoDto responseListp)
    {
        _responseListp = responseListp;
        var salidaCruda = BuildExcelCsv(filePath);
        var salidaFiltrada = ProcesarCrudo(salidaCruda);
    }


    private static StringBuilder ProcesarCrudo(StringBuilder crudo)
    { 
        var nueva = new StringBuilder();
        using var reader = new StringReader(crudo.ToString());
        string? linea;

        while ((linea = reader.ReadLine()) != null)
        {
            var resultado = TextFilterHelper.ProcesarLineaValida(linea, precioMinimo: 30m, precioMaximo: 999_999_999m);
            if (resultado != null)
                nueva.AppendLine(string.Join(" ", resultado));
        }

        return nueva;
    }

    private static void LoadExcelFile(string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheets.FirstOrDefault();

            if (worksheet is null)
            {
                Console.WriteLine("El archivo no contiene hojas de cálculo.");
                return;
            }

            var table = new DataTable(Path.GetFileNameWithoutExtension(filePath));
            var rows = worksheet.RowsUsed(XLCellsUsedOptions.AllContents).ToList();

            if (rows.Count == 0)
            {
                return;
            }

            var headerRow = rows[0];
            var maxColumnIndex = rows.Max(row => row.LastCellUsed(XLCellsUsedOptions.AllContents)?.Address.ColumnNumber ?? 0);

            if (maxColumnIndex == 0)
            {
                return;
            }

            for (var i = 1; i <= maxColumnIndex; i++)
            {
                var columnName = headerRow.Cell(i).GetString().Trim();
                if (string.IsNullOrWhiteSpace(columnName))
                {
                    columnName = $"Columna {i}";
                }

                table.Columns.Add(columnName);
            }

            for (var rowIndex = 1; rowIndex < rows.Count; rowIndex++)
            {
                var row = rows[rowIndex];
                var values = new object[table.Columns.Count];

                for (var cellIndex = 0; cellIndex < table.Columns.Count; cellIndex++)
                {
                    var cell = row.Cell(cellIndex + 1);
                    values[cellIndex] = GetCellValue(cell);
                }

                table.Rows.Add(values);
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"No se pudo cargar el archivo Excel: {ex.Message}");
        }
    }

    private static StringBuilder BuildExcelCsv(string filePath)
    {
        var csv = new StringBuilder();

        try
        {
            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheets.FirstOrDefault();

            if (worksheet is null)
            {
                return csv;
            }

            var rows = worksheet.RowsUsed(XLCellsUsedOptions.AllContents).ToList();

            if (rows.Count == 0)
            {
                return csv;
            }

            var headerRow = rows[0];
            var maxColumnIndex = rows.Max(row => row.LastCellUsed(XLCellsUsedOptions.AllContents)?.Address.ColumnNumber ?? 0);

            if (maxColumnIndex == 0)
            {
                return csv;
            }

            var headers = new List<string>();
            for (var i = 1; i <= maxColumnIndex; i++)
            {
                var columnName = headerRow.Cell(i).GetString().Trim();
                if (string.IsNullOrWhiteSpace(columnName))
                {
                    columnName = $"Columna {i}";
                }

                headers.Add(EscapeCsvValue(columnName));
            }

            csv.AppendLine(string.Join(";", headers));

            for (var rowIndex = 1; rowIndex < rows.Count; rowIndex++)
            {
                var row = rows[rowIndex];
                var values = new List<string>();

                for (var cellIndex = 0; cellIndex < headers.Count; cellIndex++)
                {
                    var cell = row.Cell(cellIndex + 1);
                    values.Add(EscapeCsvValue(GetCellValue(cell)));
                }

                csv.AppendLine(string.Join(";", values));
            }

            return csv;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo convertir el archivo Excel a CSV: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return new StringBuilder();
        }
    }

    private static string EscapeCsvValue(object value)
    {
        if (value is null || value == DBNull.Value)
        {
            return string.Empty;
        }

        var text = Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;

        if (text.Contains(';') || text.Contains('"') || text.Contains('\r') || text.Contains('\n'))
        {
            text = text.Replace("\"", "\"\"");
            return $"\"{text}\"";
        }

        return text;
    }

    private static bool IsExcelFile(string path)
    {
        var extension = Path.GetExtension(path);
        return string.Equals(extension, ".xls", StringComparison.OrdinalIgnoreCase)
            || string.Equals(extension, ".xlsx", StringComparison.OrdinalIgnoreCase);
    }

    private static object GetCellValue(IXLCell cell)
    {
        if (cell.IsEmpty())
        {
            return DBNull.Value;
        }

        return cell.DataType switch
        {
            XLDataType.Blank => DBNull.Value,
            XLDataType.Boolean => cell.GetBoolean(),
            XLDataType.Number => cell.GetDouble(),
            XLDataType.DateTime => cell.GetDateTime(),
            XLDataType.TimeSpan => cell.GetTimeSpan(),
            XLDataType.Text => cell.GetString(),
            _ => cell.GetFormattedString()
        };
    }

    private static string ResolveProveedorRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "Data", "Proveedor");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            var projectCandidate = Path.Combine(current.FullName, "AgenteUIProducto", "Data", "Proveedor");
            if (Directory.Exists(projectCandidate))
            {
                return projectCandidate;
            }

            current = current.Parent;
        }

        var fallback = Path.Combine(AppContext.BaseDirectory, "Data", "Proveedor");
        Directory.CreateDirectory(fallback);
        return fallback;
    }



}
