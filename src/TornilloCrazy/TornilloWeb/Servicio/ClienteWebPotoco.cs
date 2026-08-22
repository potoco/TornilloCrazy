using Azure;
using DataServicio.Tabla;
using Microsoft.Data.SqlTypes;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TornilloWeb.Servicio;

public static class ClienteWebPotoco
{
    const string _apiKey = "sk-proj-ns5k58OoOfWZFs1ZlLHZT3BlbkFJb8d6TcRCrLWZjSMiEFLg"; // Reemplaza con tu clave de API de OpenAI
    const string _modelo_embedding = "text-embedding-3-small";
    const string _modelo_chat = "gpt-5.6-luna";
    public static async Task BuscarDescripcionesIA(List<ProveedorMaestroTbl> preciosRevisar)
    {


        var nuevosProductosarevisar = string.Empty;

        foreach (var rec in preciosRevisar)
        {
            int posicion = preciosRevisar.IndexOf(rec) + 1;
            string caracterFinal = posicion == preciosRevisar.Count ? "" : "\n";
            nuevosProductosarevisar += $"{posicion}. {rec.Descripcion1}{caracterFinal}";
        }

        var request = new
        {
            model = _modelo_chat,
            response_format = new { type = "json_object"},
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = "Eres un asistente de catalogación de ferretería. Analiza cada producto de la lista enviada por el usuario y responde ÚNICAMENTE con un JSON que contenga una propiedad 'productos' con el array de productos normalizados. Cada elemento debe contener: id_temporal (number) con el número exacto del producto enviado, nombre_canonico (string), rubro (array de string), atributos (objeto: propiedad = valor), sinonimos (array de string), tipo de usos  (array de string)"
                },
                new
                {
                    role = "user",
                    content = nuevosProductosarevisar
                }
            }
        };

        using var httpClient = new HttpClient();
        using var msg = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        msg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
        msg.Content = JsonContent.Create(request);

        var response = await httpClient.SendAsync(msg);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var data = json.GetProperty("choices")[0].GetProperty("message").GetProperty("content");
        var doc = JsonDocument.Parse(data.GetString());
        var productos = doc.RootElement.GetProperty("productos");
        foreach (var p in productos.EnumerateArray())
        {
            string jsonProducto = p.GetRawText();
            string nombre = p.GetProperty("nombre_canonico").GetString();
            string rubro = p.GetProperty("rubro").GetRawText();
            string sinonimos = p.GetProperty("sinonimos").GetRawText();
            string tipoDeUsos = p.GetProperty("tipo de usos").GetRawText();
            string atributos = p.GetProperty("atributos").GetRawText(); 
            int id = p.GetProperty("id_temporal").GetInt32();

            preciosRevisar[id - 1].AtributosJson = atributos;
            preciosRevisar[id - 1].RubrosJson = rubro;
            preciosRevisar[id - 1].SinonimosJson = sinonimos;
            preciosRevisar[id - 1].UsosJson = tipoDeUsos;
            preciosRevisar[id - 1].NombreCanonico = nombre;
            preciosRevisar[id - 1].JsonRaw = jsonProducto;
            preciosRevisar[id - 1].TextoVectorial = CrearTextoEmbeddingNarrativo(jsonProducto);
        }
    }

    public static async Task CrearVector(List<ProveedorMaestroTbl> lisPreProDataTbls)
    {
        var inpusT = lisPreProDataTbls.OrderBy(x => x.ProveedorMaestroId).Select(x => x.TextoVectorial).ToArray();
        var request = new
        {
            model = _modelo_embedding,
            input = inpusT
        };

        using var httpClient = new HttpClient();
        using var msg = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/embeddings");
        msg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
        msg.Content = JsonContent.Create(request);

        var response = await httpClient.SendAsync(msg);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<EmbeddingResponse>(responseJson);


        if (result.Data.Count != lisPreProDataTbls.Count)
        {
            throw new Exception(
                $"Se esperaban {lisPreProDataTbls.Count} embeddings, " +
                $"pero OpenAI devolvió {result.Data.Count}."
            );
        }

        foreach (var item in result.Data)
        {
            if (item.Index < 0 || item.Index >= lisPreProDataTbls.Count)
            {
                throw new Exception($"Index inválido: {item.Index}");
            }

            var itemPrecio = lisPreProDataTbls[item.Index];
            itemPrecio.VectorEmbedding = new SqlVector<float>(item.Embedding.ToArray()); 

            // Guardar texto + embedding
        }


        //var list = new List<float>();
        //foreach (var v in data.EnumerateArray())
        //    list.Add((float)v.GetDouble());



    }

    public static async Task<float[]> CrearVectorBusquedaUsuarioAsync(string textoUsuario)
    {
        var request = new
        {
            model = _modelo_embedding,
            input = textoUsuario
        };

        using var httpClient = new HttpClient();
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
        return list.ToArray();
    }


    private static string CrearTextoEmbeddingNarrativo(string jsonProducto)
    {
        if (string.IsNullOrWhiteSpace(jsonProducto))
            return string.Empty;

        using var doc = JsonDocument.Parse(jsonProducto);
        var root = doc.RootElement;

        var sb = new StringBuilder();

        // 1. Nombre del producto
        if (root.TryGetProperty("nombre_canonico", out var nombre))
        {
            var textoNombre = ValorComoTexto(nombre);
            if (!string.IsNullOrWhiteSpace(textoNombre))
            {
                sb.AppendLine($"Producto: {textoNombre}.");
            }
        }

        // 2. Atributos (maneja números, strings y nulos de forma segura)
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

        // 3. Rubros (soporta si viene array o string único)
        ProcesarSeccion(sb, "Pertenece a los rubros: ", root, "rubro");

        // 4. Sinónimos
        ProcesarSeccion(sb, "También conocido como: ", root, "sinonimos");

        // 5. Usos
        ProcesarSeccion(sb, "Usos habituales: ", root, "tipo de usos");

        return sb.ToString().Trim();
    }
    private static void ProcesarSeccion(StringBuilder sb, string prefijo, JsonElement root, string propiedad)
    {
        if (!root.TryGetProperty(propiedad, out var elemento))
            return;

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
            // En caso de que la IA haya devuelto un string en lugar de un array
            var txt = ValorComoTexto(elemento);
            if (!string.IsNullOrWhiteSpace(txt))
                items.Add(txt);
        }

        if (items.Count > 0)
        {
            sb.Append(prefijo);
            sb.AppendLine(string.Join(", ", items) + ".");
        }
    }
    private static string? ValorComoTexto(JsonElement elemento)
    {
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
    private static string PrimeraMayus(string clave)
    {
        if (string.IsNullOrEmpty(clave))
            return clave;

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





