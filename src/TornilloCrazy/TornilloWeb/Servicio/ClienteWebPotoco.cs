using Azure;
using DataServicio.Tabla;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Data.SqlTypes;
using Serilog;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace TornilloWeb.Servicio;

public class ClienteWebPotoco
{
    private readonly IConfiguration _config;
    public ClienteWebPotoco(IConfiguration config)
    {
        _config = config;
        _apiKey = _config["IA:OpenIA:Token"];
        _log.Debug("ClienteWebPotoco inicializado.");

    }
    const int _tiempoMaximoEsperaEnMinutos = 5; // 5 minutos
    string _apiKey  = "";
    const string _modelo_embedding = "text-embedding-3-large"; //"text-embedding-3-large  -small";
    const string _modelo_chat = "gpt-5.6-luna";
    private  readonly Serilog.ILogger _log = Log.ForContext(typeof(ClienteWebPotoco));
    public async Task BuscarDescripcionesIA(ModeloBuscarDescripcion dataInstr)
    {
        _log.Information("Inicio BuscarDescripcionesIA. Cantidad de productos: {Cantidad}", dataInstr.PreciosRevisar.Count);

        string RemoveFirstAndLastChar(string text)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= 2)
                return string.Empty;
            return text[1..^1];
        }


    string instrunccionDeveloper = dataInstr.InstruccionDeveloper;
        string instrunccionUsuario = dataInstr.InstruccionUsuario;


        var request = new JsonObject
        {
            ["model"] = _modelo_chat,
            ["input"] = new JsonArray
            {
                new JsonObject
                {
                    ["role"] = "developer",
                    ["content"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["type"] = "input_text",
                            ["text"] = instrunccionDeveloper
                        }
                    }
                },
                new JsonObject
                {
                    ["role"] = "user",
                    ["content"] = instrunccionUsuario
                }
            },
            ["text"] = new JsonObject
            {
                ["format"] = new JsonObject
                {
                    ["type"] = "json_object"
                },
                ["verbosity"] = "medium"
            }
        };




        var preciosRevisar = dataInstr.PreciosRevisar;

        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromMinutes(_tiempoMaximoEsperaEnMinutos); // Ajusta el tiempo de espera según tus necesidades
        using var msg = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/responses");
        msg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
        msg.Content = JsonContent.Create(request);

        var response = await httpClient.SendAsync(msg);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        _log.Debug("Respuesta de descripciones IA recibida correctamente.");


        var message = json
            .GetProperty("output")
            .EnumerateArray()
            .First(x => x.GetProperty("type").GetString() == "message");

        var outputText = message
            .GetProperty("content")
            .EnumerateArray()
            .First(x => x.GetProperty("type").GetString() == "output_text");

        var data = outputText.GetProperty("text");


        var doc = JsonDocument.Parse(data.GetString());
        var productos = doc.RootElement.GetProperty("productos");
        var procesados = 0;
        foreach (var p in productos.EnumerateArray())
        {
            int id = p.GetProperty("id_temporal").GetInt32();
            try
            {
                string jsonProducto = p.GetRawText();
                string nombre = p.GetProperty("nombre_canonico").GetString();
                string rubro = p.GetProperty("rubro").GetRawText();
                string sinonimos = p.GetProperty("sinonimos").GetRawText();
                string tipoDeUsos = p.GetProperty("tipo_de_usos").GetRawText();
                string atributos = p.GetProperty("atributos").GetRawText();
                string descDet = RemoveFirstAndLastChar(p.GetProperty("descripcion_detallada").GetRawText());

                preciosRevisar[id - 1].AtributosJson = atributos;
                preciosRevisar[id - 1].RubrosJson = rubro;
                preciosRevisar[id - 1].SinonimosJson = sinonimos;
                preciosRevisar[id - 1].UsosJson = tipoDeUsos;
                preciosRevisar[id - 1].NombreCanonico = nombre;
                preciosRevisar[id - 1].JsonRaw = jsonProducto;
                preciosRevisar[id - 1].DescripcionDetallada = descDet;
                preciosRevisar[id - 1].TextoVectorial = CrearTextoEmbeddingNarrativo(jsonProducto);
                preciosRevisar[id - 1].ErrorMensaje = "sin errores";
                procesados++;
            }
            catch (Exception ex)
            {
                preciosRevisar[id - 1].ErrorMensaje = $"Error procesando producto: {ex.Message}";
                _log.Error(ex, "Error procesando producto con id temporal {IdTemporal}.", id);
            }

        }

        _log.Information("Fin BuscarDescripcionesIA. Productos procesados correctamente: {Procesados}", procesados);
    }
    public async Task CrearVector(ModeloEmbeddingProducto dataModelo)
    {
        _log.Information("Inicio CrearVector. Cantidad de entradas: {Cantidad}", dataModelo.InstruccionUsuario.Length);
        var request = new
        {
            model = _modelo_embedding,
            input = dataModelo.InstruccionUsuario,
            dimensions = 1536
        };

        var lisPreProDataTbls = dataModelo.PreciosRevisar;

        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromMinutes(_tiempoMaximoEsperaEnMinutos); // Ajusta el tiempo de espera según tus necesidades
        using var msg = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/embeddings");
        msg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
        msg.Content = JsonContent.Create(request);

        var response = await httpClient.SendAsync(msg);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<EmbeddingResponse>(responseJson);
        _log.Debug("Embeddings recibidos desde API.");


        if (result.Data.Count != lisPreProDataTbls.Count)
        {
            _log.Error("Cantidad de embeddings inválida. Esperados: {Esperados}. Recibidos: {Recibidos}", lisPreProDataTbls.Count, result.Data.Count);
            throw new Exception(
                $"Se esperaban {lisPreProDataTbls.Count} embeddings, " +
                $"pero OpenAI devolvió {result.Data.Count}."
            );
        }

        foreach (var item in result.Data)
        {
            if (item.Index < 0 || item.Index >= lisPreProDataTbls.Count)
            {
                _log.Error("Índice de embedding inválido: {Index}", item.Index);
                throw new Exception($"Index inválido: {item.Index}");
            }

            var itemPrecio = lisPreProDataTbls[item.Index];
            itemPrecio.VectorEmbedding = new SqlVector<float>(item.Embedding.ToArray()); 
        }

        _log.Information("Fin CrearVector. Embeddings asignados: {Cantidad}", result.Data.Count);


    }
    public async Task<float[]> CrearVectorBusquedaUsuarioAsync(string textoUsuario)
    {
        _log.Information("Inicio CrearVectorBusquedaUsuarioAsync. Longitud de texto: {Longitud}", textoUsuario?.Length ?? 0);
        var request = new
        {
            model = _modelo_embedding,
            input = textoUsuario,
            dimensions = 1536
        };

        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromMinutes(_tiempoMaximoEsperaEnMinutos); // Ajusta el tiempo de espera según tus necesidades
        using var msg = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/embeddings");
        msg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
        msg.Content = JsonContent.Create(request);
        var response = await httpClient.SendAsync(msg);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var data = json.GetProperty("data")[0].GetProperty("embedding");
        var list = new List<float>();
        foreach (var v in data.EnumerateArray())
            list.Add((float)v.GetDouble());
        _log.Information("Fin CrearVectorBusquedaUsuarioAsync. Dimensiones generadas: {Cantidad}", list.Count);
        return list.ToArray();
    }

    private  string CrearTextoEmbeddingNarrativo(string jsonProducto)
    {
        _log.Debug("Inicio CrearTextoEmbeddingNarrativo.");
        if (string.IsNullOrWhiteSpace(jsonProducto))
        {
            _log.Warning("CrearTextoEmbeddingNarrativo recibió JSON vacío.");
            return string.Empty;
        }

        using var doc = JsonDocument.Parse(jsonProducto);
        var root = doc.RootElement;

        var sb = new StringBuilder();

        if (root.TryGetProperty("nombre_canonico", out var nombre))
        {
            var textoNombre = ValorComoTexto(nombre);
            if (!string.IsNullOrWhiteSpace(textoNombre))
            {
                sb.AppendLine($"Producto: {textoNombre}.");
            }
        }

        if (root.TryGetProperty("atributos", out var atributos) && atributos.ValueKind == JsonValueKind.Object)
        {
            foreach (var attr in atributos.EnumerateObject())
            {
                var valor = ValorComoTexto(attr.Value);
                if (!string.IsNullOrWhiteSpace(valor))
                {
                    sb.AppendLine($"{PrimeraMayus(attr.Name)}: {valor}.");
                }
            }
        }

        ProcesarSeccion(sb, "Pertenece a los rubros: ", root, "rubro");
        ProcesarSeccion(sb, "También conocido como: ", root, "sinonimos");
        ProcesarSeccion(sb, "Usos habituales: ", root, "tipo_de_usos");

        var salida = sb.ToString().Trim();
        _log.Debug("Fin CrearTextoEmbeddingNarrativo. Longitud de salida: {Longitud}", salida.Length);
        return salida;
    }
    private  void ProcesarSeccion(StringBuilder sb, string prefijo, JsonElement root, string propiedad)
    {
        _log.Debug("ProcesarSeccion para propiedad {Propiedad}.", propiedad);
        if (!root.TryGetProperty(propiedad, out var elemento))
        {
            _log.Debug("Propiedad {Propiedad} no encontrada.", propiedad);
            return;
        }

        var items = new List<string>();

        if (elemento.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in elemento.EnumerateArray())
            {
                var txt = ValorComoTexto(item);
                if (!string.IsNullOrWhiteSpace(txt))
                    items.Add(txt);
            }
        }
        else
        {
            var txt = ValorComoTexto(elemento);
            if (!string.IsNullOrWhiteSpace(txt))
                items.Add(txt);
        }

        if (items.Count > 0)
        {
            sb.Append(prefijo);
            sb.AppendLine(string.Join(", ", items) + ".");
            _log.Debug("ProcesarSeccion añadió {Cantidad} ítems para propiedad {Propiedad}.", items.Count, propiedad);
        }
    }
    private  string? ValorComoTexto(JsonElement elemento)
    {
        _log.Debug("ValorComoTexto invocado para tipo {Tipo}.", elemento.ValueKind);
        return elemento.ValueKind switch
        {
            JsonValueKind.String => elemento.GetString()?.Trim(),
            JsonValueKind.Number => elemento.GetRawText().Trim(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            _ => elemento.GetRawText().Trim()
        };
    }
    private  string PrimeraMayus(string clave)
    {
        _log.Debug("PrimeraMayus invocado.");
        if (string.IsNullOrEmpty(clave))
        {
            _log.Debug("PrimeraMayus recibió cadena vacía.");
            return clave;
        }

        var limpia = clave.Replace('_', ' ');
        return string.Concat(char.ToUpperInvariant(limpia[0]), limpia[1..]);
    }

}


public class EmbeddingData
{
    [JsonPropertyName("index")]     public int Index { get; set; }
    [JsonPropertyName("embedding")] public List<float> Embedding { get; set; }
}
public class EmbeddingResponse
{
    [JsonPropertyName("data")]      public List<EmbeddingData> Data { get; set; }
}





