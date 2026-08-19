

using System.Globalization;
using System.Text.RegularExpressions;

namespace AgenteUIProducto;
public record ProductoDetectado(
    string? Codigo,
    string Descripcion,
    decimal Precio,
    string LineaCruda 
);

public static class TextFilterHelper
{
    public const decimal PrecioMaximoDefault = 50_000m;

    /// <summary>
    /// Procesa la línea ubicando la descripción legible y tomando el primer número
    /// posterior que tenga "olor a precio" dentro del rango válido.
    /// </summary>
    public static ProductoDetectado? ProcesarLineaValida(
        string? input,
        decimal precioMaximo = PrecioMaximoDefault,
        decimal precioMinimo = 0.01m)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        // Separar manteniendo el orden de columnas
        var partes = input.Split([',', ';'], StringSplitOptions.TrimEntries)
                          .Select(LimpiarToken)
                          .Where(p => !string.IsNullOrEmpty(p))
                          .ToArray();

        if (partes.Length == 0)
            return null;

        int indiceDescripcion = -1;
        string? descripcion = null;

        // 1. Buscar la columna ancla: la descripción legible en español
        for (int i = 0; i < partes.Length; i++)
        {
            if (EsDescripcionEntendible(partes[i]))
            {
                indiceDescripcion = i;
                descripcion = partes[i];
                break;
            }
        }

        // Si no hay descripción entendible, descartamos la línea
        if (indiceDescripcion == -1 || string.IsNullOrEmpty(descripcion))
            return null;

        // Si había una columna previa a la descripción, la tomamos como código (opcional)
        string? codigo = indiceDescripcion > 0 ? partes[0] : null;

        // 2. Buscar el PRIMER número posterior a la descripción con "olor a precio"
        for (int i = indiceDescripcion + 1; i < partes.Length; i++)
        {
            if (TieneOlorAPrecio(partes[i], precioMinimo, precioMaximo, out decimal precioDetectado))
            {
                return new ProductoDetectado(
                    Codigo: codigo,
                    Descripcion: descripcion,
                    Precio: precioDetectado,
                    LineaCruda: input
                );
            }
        }

        // Si no hubo ningún precio válido posterior, no es una línea válida
        return null;
    }

    private static bool TieneOlorAPrecio(string token, decimal min, decimal max, out decimal precio)
    {
        precio = 0;

        // Limpiar símbolos de moneda si vinieran ($ / USD / ARS / espacios)
        string limpio = Regex.Replace(token, @"[^\d,\.]", "").Trim();

        if (string.IsNullOrEmpty(limpio))
            return false;

        // Normalizar comas decimales a punto
        string normalizado = limpio.Replace(',', '.');

        // Manejo de posibles separadores de miles (ej: 18.169,20 -> 18169.20)
        if (limpio.Contains('.') && limpio.Contains(','))
        {
            // Caso estándar latino: 1.250,50
            if (limpio.IndexOf('.') < limpio.IndexOf(','))
                normalizado = limpio.Replace(".", "").Replace(',', '.');
            // Caso estándar anglosajón: 1,250.50
            else
                normalizado = limpio.Replace(",", "");
        }

        if (!decimal.TryParse(normalizado, NumberStyles.Any, CultureInfo.InvariantCulture, out precio))
            return false;

        // Reglas de "olor a precio":
        // 1. Debe estar en el rango de búsqueda
        if (precio < min || precio > max)
            return false;

        // 2. Filtro de falsos positivos comunes posteriores (ej: años como 2024/2025/2026 sin decimales)
        if (precio >= 2020 && precio <= 2030 && !token.Contains('.') && !token.Contains(','))
            return false;

        return true;
    }

    private static string LimpiarToken(string token)
    {
        return token.Trim('"', '\'', ' ')
                    .Replace("\"\"", "\"")
                    .Trim();
    }

    private static bool EsDescripcionEntendible(string texto)
    {
        if (texto.Length < 4)
            return false;

        // Si es puramente numérico, descartar
        string sinSimbolos = Regex.Replace(texto, @"[^\d,\.]", "");
        if (sinSimbolos.Length == texto.Length && decimal.TryParse(texto.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            return false;

        // Descartar códigos alfanuméricos compactos sin espacios (ej: "A0303391", "SKU-9901")
        if (!texto.Contains(' ') && Regex.IsMatch(texto, @"^[A-Z0-9\-_]+$", RegexOptions.IgnoreCase))
            return false;

        var palabras = texto.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int cantidadLetras = texto.Count(char.IsLetter);

        // Al menos 2 palabras y carga alfabética
        if (palabras.Length >= 2 && cantidadLetras >= 4)
            return true;

        // O una sola palabra larga (ej: "PORCELANATO", "IMPERMEABILIZANTE")
        if (palabras.Length == 1 && cantidadLetras > 4 && (double)cantidadLetras / texto.Length > 0.8)
            return true;

        return false;
    }
}
