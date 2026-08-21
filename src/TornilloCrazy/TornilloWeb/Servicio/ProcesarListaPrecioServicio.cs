using Azure.Core;
using ClosedXML.Excel;
using DataServicio.Servicio;
using System.Data;
using System.Text;

namespace TornilloWeb.Servicio;

public class ProcesarListaPrecioServicio
{
    private readonly IWebHostEnvironment _environment;

    public ProcesarListaPrecioServicio(IWebHostEnvironment environment)
    {
        _environment = environment;
    }   

    public StringBuilder BuildExcelCsv(string fileName, int proveedorId)
    {
        var carpetaDestino = Path.Combine(_environment.ContentRootPath,"AppData","Proveedores",$"Proveedor-{proveedorId}", fileName);
        var csv = new StringBuilder();

        try
        {
            using var workbook = new XLWorkbook(carpetaDestino);
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
            Console.WriteLine($"No se pudo convertir el archivo Excel a CSV: {ex.Message}");
            return new StringBuilder();
        }
    }
    private  string EscapeCsvValue(object value)
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
    private  object GetCellValue(IXLCell cell)
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

}
