using AgenteUIProducto.Helper;
using ClosedXML.Excel;
using System.Data;
using System.Text;

namespace AgenteUIProducto;

public partial class Form1 : Form
{
    private readonly string _rootPath;

    public Form1()
    {
        InitializeComponent();
        this.Load += Form1_Load;
        _rootPath = ResolveProveedorRoot();
    }

    private void Form1_Load(object? sender, EventArgs e)
    {
        textBox1.Dock = DockStyle.Fill;
        textBox2.Dock = DockStyle.Fill;
        textBox2.BackColor = Color.WhiteSmoke;
    }



    private void menuCargarArchivos_Click(object sender, EventArgs e)
    {
        textBox1.Text = string.Empty;
        textBox2.Text = string.Empty;
        using var openFileDialog = new OpenFileDialog
        {
            Title = "Seleccionar archivo Excel",
            Filter = "Archivos Excel (*.xls;*.xls)|*.xlsx;*.xlsx",
            Multiselect = true,
            RestoreDirectory = true
        };

        if (openFileDialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        foreach (var filePath in openFileDialog.FileNames)
        {
            if (!IsExcelFile(filePath))
            {
                continue;
            }

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(_rootPath))
            {
                Directory.CreateDirectory(_rootPath);
            }

            // LoadDirectoryTree();
            //LoadExcelFile(filePath);
            var salidaCruda = BuildFromExcel.ConstruirItemsDesdeExcel(filePath);  // BuildExcelCsv(filePath);
            textBox1.Text = salidaCruda.ToString();

            //var salidaFiltrada = ProcesarCrudo(salidaCruda);

            string salidaFiltrada = "";
            foreach (var item in salidaCruda)
            {
                salidaFiltrada += item.ToString() + Environment.NewLine;
            }
            textBox2.Text = salidaFiltrada.ToString();

            break;
        }
    }

    private void treeViewArchivos_AfterSelect(object sender, TreeViewEventArgs e)
    {
        if (e.Node is null || e.Node.Tag is not string path || !File.Exists(path))
        {
            return;
        }

        if (IsExcelFile(path))
        {
            LoadExcelFile(path);
        }
    }

    private void LoadDirectoryTree()
    {

        if (!Directory.Exists(_rootPath))
        {
            Directory.CreateDirectory(_rootPath);
        }

        var rootNode = new TreeNode("Data / Proveedor")
        {
            Tag = _rootPath,
            Name = "root"
        };

        PopulateDirectoryNode(rootNode, _rootPath);
        rootNode.Expand();
    }


    private StringBuilder ProcesarCrudo(StringBuilder crudo)
    {
        var nueva = new StringBuilder();
        using var reader = new StringReader(crudo.ToString());
        string? linea;

        while ((linea = reader.ReadLine()) != null)
        {
            var resultado = TextFilterHelper.ProcesarLineaValida(linea, precioMinimo: 50m, precioMaximo: 999_999m);
            if (resultado != null)
                nueva.AppendLine(string.Join(" ", resultado));
        }

        return nueva;
    }

    private static void PopulateDirectoryNode(TreeNode node, string directoryPath)
    {
        foreach (var directory in Directory.GetDirectories(directoryPath).OrderBy(d => d, StringComparer.OrdinalIgnoreCase))
        {
            var directoryInfo = new DirectoryInfo(directory);
            var childNode = new TreeNode(directoryInfo.Name)
            {
                Tag = directoryInfo.FullName,
                Name = directoryInfo.Name
            };

            PopulateDirectoryNode(childNode, directoryInfo.FullName);

            foreach (var file in Directory.GetFiles(directoryInfo.FullName).OrderBy(f => f, StringComparer.OrdinalIgnoreCase).Where(IsExcelFile))
            {
                childNode.Nodes.Add(new TreeNode(Path.GetFileName(file))
                {
                    Tag = file,
                    Name = Path.GetFileName(file)
                });
            }

            if (childNode.Nodes.Count > 0 || Directory.GetDirectories(directoryInfo.FullName).Length > 0)
            {
                node.Nodes.Add(childNode);
            }
        }

        foreach (var file in Directory.GetFiles(directoryPath).OrderBy(f => f, StringComparer.OrdinalIgnoreCase).Where(IsExcelFile))
        {
            node.Nodes.Add(new TreeNode(Path.GetFileName(file))
            {
                Tag = file,
                Name = Path.GetFileName(file)
            });
        }
    }

    private void LoadExcelFile(string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheets.FirstOrDefault();

            if (worksheet is null)
            {
                MessageBox.Show("El archivo no contiene hojas de cálculo.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            MessageBox.Show($"No se pudo cargar el archivo Excel: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                for (var cellIndex = 0; cellIndex < maxColumnIndex; cellIndex++)
                {
                    var cell = row.Cell(cellIndex + 1);
                    string escapedValue = EscapeCsvValue(GetCellValue(cell));
                    if(!string.IsNullOrEmpty(escapedValue))
                    {
                        values.Add(escapedValue);
                    }
                }
                if(values != null && values.Count > 0)
                {
                    csv.AppendLine(string.Join(";", values));
                }
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

        //if (text.Contains(';') || text.Contains('"') || text.Contains('\r') || text.Contains('\n'))
        //{
        //    text = text.Replace("\"", "\"\"");
        //    return $"\"{text}\"";
        //}

        text = text.Replace(";", " ");
        text = text.Replace("'", "");
        text = text.Replace("\"", "");
        text = text.Replace("\r", "");
        text = text.Replace("\n", "");


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