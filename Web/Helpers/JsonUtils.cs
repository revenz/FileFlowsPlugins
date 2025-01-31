using System.Text.Json;

namespace FileFlows.Web.Helpers;

/// <summary>
/// Provides utility methods for working with JSON data, including the ability to
/// recursively deserialize JSON strings into .NET objects such as <see cref="Dictionary{string, object}"/>.
/// </summary>
public static class JsonUtils
{
    /// <summary>
    /// Recursively deserializes a JSON string into an object where:
    /// - JSON objects are represented as <see cref="Dictionary{string, object}"/>
    /// - JSON arrays are represented as <see cref="List{object}"/>
    /// - JSON primitives are represented as their corresponding .NET types (e.g., int, string, bool).
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>
    /// An object representing the JSON structure. The return type will be one of the following:
    /// - <see cref="Dictionary{string, object}"/> for JSON objects.
    /// - <see cref="List{object}"/> for JSON arrays.
    /// - Primitive .NET types (e.g., int, string, bool) for JSON values.
    /// Returns <c>null</c> if the input JSON string is null or empty.
    /// </returns>
    /// <exception cref="JsonException">Thrown when the input JSON is invalid.</exception>
    public static object? DeserializeToDictionary(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        using var document = JsonDocument.Parse(json);
        return ConvertElement(document.RootElement);
    }

    /// <summary>
    /// Recursively converts a <see cref="JsonElement"/> into a corresponding .NET object.
    /// </summary>
    /// <param name="element">The <see cref="JsonElement"/> to convert.</param>
    /// <returns>
    /// An object representing the JSON element. The return type will be one of the following:
    /// - <see cref="Dictionary{string, object}"/> for JSON objects.
    /// - <see cref="List{object}"/> for JSON arrays.
    /// - Primitive .NET types (e.g., int, string, bool) for JSON values.
    /// Returns <c>null</c> for JSON null values or unsupported types.
    /// </returns>
    private static object? ConvertElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => ConvertObject(element),
            JsonValueKind.Array => ConvertArray(element),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var longValue) ? longValue : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => null
        };
    }

    /// <summary>
    /// Converts a JSON object to a <see cref="Dictionary{string, object}"/>.
    /// </summary>
    /// <param name="element">The <see cref="JsonElement"/> representing the JSON object.</param>
    /// <returns>
    /// A <see cref="Dictionary{string, object}"/> where keys are JSON property names and values are their
    /// corresponding .NET representations.
    /// </returns>
    private static Dictionary<string, object?> ConvertObject(JsonElement element)
    {
        var dictionary = new Dictionary<string, object?>();
        foreach (var property in element.EnumerateObject())
        {
            dictionary[property.Name] = ConvertElement(property.Value);
        }
        return dictionary;
    }

    /// <summary>
    /// Converts a JSON array to a <see cref="List{object}"/>.
    /// </summary>
    /// <param name="element">The <see cref="JsonElement"/> representing the JSON array.</param>
    /// <returns>
    /// A <see cref="List{object}"/> containing the .NET representations of the JSON array elements.
    /// </returns>
    private static List<object?> ConvertArray(JsonElement element)
    {
        var list = new List<object?>();
        foreach (var item in element.EnumerateArray())
        {
            list.Add(ConvertElement(item));
        }
        return list;
    }
}
