using DocumentFormat.OpenXml.ExtendedProperties;
using Microsoft.Data.SqlTypes;
using System.Numerics.Tensors;
using System.Security.AccessControl;

namespace TornilloWeb.Servicio;

public static class VectorHelper
{
    // Si tu propiedad Vector es SqlVector<float> o SqlVector<float>?
    public static float Similitud(SqlVector<float>? a, SqlVector<float>? b)
    {
        if (!a.HasValue || !b.HasValue)
            return 0f;

        return TensorPrimitives.CosineSimilarity(a.Value.Memory.Span, b.Value.Memory.Span);
    }

    // Si tu propiedad Vector es ReadOnlyMemory<float>
    public static float Similitud(ReadOnlyMemory<float> a, ReadOnlyMemory<float> b)
    {
        return TensorPrimitives.CosineSimilarity(a.Span, b.Span);
    }

    // Si tu propiedad Vector es float[]
    public static float Similitud(float[]? a, float[]? b)
    {
        if (a is null || b is null) return 0f;
        return TensorPrimitives.CosineSimilarity(a.AsSpan(), b.AsSpan());
    }
}


