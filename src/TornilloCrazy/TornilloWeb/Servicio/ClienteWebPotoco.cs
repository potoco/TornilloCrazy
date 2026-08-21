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
    /*
POST https://api.openai.com/v1/chat/completions 
Content-Type: application/json
Authorization: Bearer sk-proj-ns5k58OoOfWZFs1ZlLHZT3BlbkFJb8d6TcRCrLWZjSMiEFLg
  
{
  "model": {{modelo}},
  "response_format": { "type": "json_object" },
  "messages": [
    {
      "role": "system",
      "content": "Eres un asistente de catalogación de ferretería. Analiza cada producto de la lista enviada por el usuario y responde ÚNICAMENTE con un JSON que contenga una propiedad 'productos' con el array de productos normalizados. Cada elemento debe contener: id_temporal (number) con el número exacto del producto enviado, nombre_canonico (string), rubro (array de string), atributos (objeto: propiedad = valor), sinonimos (array de string), tipo de usos  (array de string)"
    },
    {
      "role": "user",
      "content" :"1. ABRAZADERA C/CREMALL REF12MM 60-80 ABRAZADERAS PERFECTO\n2. ADAPTADOR TANQUE FUSION 20 IPS\n3. ABRAZADERA C/CREMALL STD 9MM 12-22 ABRAZADERAS PERFECTO\n4. AEROSOL BRILL BLANCO 250 CC TESUAVE\n5. ADAPTADOR TANQUE FUSION 32 IPS\n6. AEROSOL BRILL BLANCO 440 CC TERSUAVE" 
    }
  ]
}     
     */

    public static async Task BuscarDescripcionesIA(List<LisPreProDataTbl> preciosRevisar)
    {


        //string respuesta = await leerJsonStringDesdeDisco();
        //if (respuesta == null || respuesta.Length == 0)
        //{
        //    respuesta = data.ToString();
        //}
        //var jsonA = await JsonSerializer.DeserializeAsync<JsonElement>(new MemoryStream(Encoding.UTF8.GetBytes(respuesta)));
        //var contenidoTxt = jsonA.GetProperty("choices")[0].GetProperty("message").GetProperty("content");



        var nuevosProductosarevisar = string.Empty;

        foreach (var rec in preciosRevisar)
        {
            int posicion = preciosRevisar.IndexOf(rec) + 1;
            string caracterFinal = posicion == preciosRevisar.Count ? "" : "\n";
            nuevosProductosarevisar += $"{posicion}. {rec.Descripcion1}{caracterFinal}";
        }

        var request = new
        {
            model = "gpt-5.6-luna",
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
    public static async Task BuscarDescripcionesIA_MENTIRA(List<LisPreProDataTbl> preciosRevisar)
    {


        string respuesta = await leerJsonStringDesdeDisco();
        var json = await JsonSerializer.DeserializeAsync<JsonElement>(new MemoryStream(Encoding.UTF8.GetBytes(respuesta)));
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

    public static async Task CrearVector(List<LisPreProDataTbl> lisPreProDataTbls)
    {
        var inpusT = lisPreProDataTbls.OrderBy(x => x.LisPreProDataId).Select(x => x.TextoVectorial).ToArray();
        var request = new
        {
            model = "text-embedding-3-small",
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

    private static async Task<string> leerJsonStringDesdeDisco()
    {
        string archivoTest = Path.Combine(AppContext.BaseDirectory, "AppData", "test.json");
        var salida = await System.IO.File.ReadAllTextAsync(archivoTest);
        return salida;  
    }

    private static string CrearTextoEmbeddingNarrativo(string jsonProducto)
    {
        using var doc = JsonDocument.Parse(jsonProducto);
        var root = doc.RootElement;

        var sb = new StringBuilder();

        // Nombre del producto
        if (root.TryGetProperty("nombre_canonico", out var nombre))
        {
            sb.AppendLine($"Producto: {nombre.GetString()}.");
        }

        // Atributos narrativos
        if (root.TryGetProperty("atributos", out var atributos))
        {
            foreach (var attr in atributos.EnumerateObject())
            {
                sb.AppendLine($"{PrimeraMayus(attr.Name)}: {attr.Value.GetString()}.");
            }
        }

        // Rubros
        if (root.TryGetProperty("rubro", out var rubro))
        {
            sb.Append("Pertenece a los rubros: ");
            sb.AppendLine(string.Join(", ", rubro.EnumerateArray().Select(x => x.GetString())) + ".");
        }

        // Sinónimos
        if (root.TryGetProperty("sinonimos", out var sinonimos))
        {
            sb.Append("También conocido como: ");
            sb.AppendLine(string.Join(", ", sinonimos.EnumerateArray().Select(x => x.GetString())) + ".");
        }

        // Usos
        if (root.TryGetProperty("tipo de usos", out var usos))
        {
            sb.Append("Usos habituales: ");
            sb.AppendLine(string.Join(", ", usos.EnumerateArray().Select(x => x.GetString())) + ".");
        }

        return sb.ToString().Trim();
    }

    private static string PrimeraMayus(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return texto;
        return char.ToUpper(texto[0]) + texto.Substring(1);
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