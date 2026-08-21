using ClosedXML.Excel;
using DataServicio.Servicio;
using System.Data;
using System.Text;

namespace TornilloWeb.Servicio;

public class ProcesarListaPrecioServicio
{
    private readonly ProveedorService _proveedorService;

    public ProcesarListaPrecioServicio(ProveedorService proveedorService)
    {
        _proveedorService = proveedorService;
    }   

    private void Procesar(string filePath, ListaPrecioArchivoDto responseListp)
    {
        Task.Run(() => 
        {
            var salidaCruda = BuildExcelCsv(filePath);
            PoblarListaPrecio(salidaCruda, responseListp);
        });
    }


    private async Task PoblarListaPrecio(StringBuilder crudo, ListaPrecioArchivoDto listaProveedorDto)
    {
        var nueva = new StringBuilder();
        using var reader = new StringReader(crudo.ToString());
        string? linea;
        int idPrecioLista = listaProveedorDto.ListaPrecioProveedorId;
        while ((linea = reader.ReadLine()) != null)
        {
            var resultado = TextFilterHelper.ProcesarLineaValida(linea, precioMinimo: 30m, precioMaximo: 999_999_999m);
            if (resultado != null)
            {
                await _proveedorService.AgregarItemPrecioAsync(idPrecioLista, resultado.Descripcion, resultado.LineaCruda);
            }
        }
    }

    private  StringBuilder BuildExcelCsv(string filePath)
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
    internal async Task<ListaPrecioArchivoDto?> GuardarAsync(string rutaDestino, string nombreArchivoGuid, string nombreArchivoOriginal, int proveedorId)
    {
        var responseListp = await _proveedorService.RegistrarListaPrecioAsync(
            proveedorId,
            nombreArchivoGuid,
            nombreArchivoOriginal);
        if (responseListp != null && responseListp.ListaPrecioProveedorId > 0)
        {
            Procesar(rutaDestino, responseListp);
        }
        return responseListp;
    }
}
