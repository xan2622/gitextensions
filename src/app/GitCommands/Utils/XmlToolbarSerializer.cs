using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace GitCommands.Utils;

/// <summary>
/// XML serializer for toolbar configuration data.
/// Uses DataContractSerializer for compatibility with [DataContract] attributes.
/// </summary>
public static class XmlToolbarSerializer
{
    /// <summary>
    /// Serializes an object to XML string.
    /// </summary>
    /// <typeparam name="T">Type of object to serialize (must have [DataContract] attribute)</typeparam>
    /// <param name="obj">Object to serialize</param>
    /// <returns>XML string representation</returns>
    public static string Serialize<T>(T? obj) where T : class
    {
        if (obj is null)
        {
            return string.Empty;
        }

        DataContractSerializer serializer = new(typeof(T));
        using MemoryStream stream = new();
        using XmlWriter writer = XmlWriter.Create(stream, new XmlWriterSettings
        {
            Indent = true,
            Encoding = Encoding.UTF8,
            OmitXmlDeclaration = true
        });

        serializer.WriteObject(writer, obj);
        writer.Flush();

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// Deserializes an XML string to an object.
    /// </summary>
    /// <typeparam name="T">Type of object to deserialize (must have [DataContract] attribute)</typeparam>
    /// <param name="xml">XML string to deserialize</param>
    /// <returns>Deserialized object or null if deserialization fails</returns>
    public static T? Deserialize<T>(string xml) where T : class
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            System.Diagnostics.Debug.WriteLine($"[XmlToolbarSerializer.Deserialize<{typeof(T).Name}>] XML is empty");
            return null;
        }

        try
        {
            System.Diagnostics.Debug.WriteLine($"[XmlToolbarSerializer.Deserialize<{typeof(T).Name}>] Attempting XML deserialization, length: {xml.Length}");
            DataContractSerializer serializer = new(typeof(T));
            using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));
            var result = (T?)serializer.ReadObject(stream);
            System.Diagnostics.Debug.WriteLine($"[XmlToolbarSerializer.Deserialize<{typeof(T).Name}>] XML deserialization SUCCESS");
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[XmlToolbarSerializer.Deserialize<{typeof(T).Name}>] XML deserialization FAILED: {ex.Message}");

            // If deserialization fails, try to deserialize from JSON for backward compatibility
            return TryDeserializeFromJson<T>(xml);
        }
    }

    /// <summary>
    /// Tries to deserialize from JSON format for backward compatibility with existing settings.
    /// </summary>
    private static T? TryDeserializeFromJson<T>(string data) where T : class
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[XmlToolbarSerializer.TryDeserializeFromJson<{typeof(T).Name}>] Attempting JSON deserialization for backward compatibility");
            var result = JsonSerializer.Deserialize<T>(data);
            System.Diagnostics.Debug.WriteLine($"[XmlToolbarSerializer.TryDeserializeFromJson<{typeof(T).Name}>] JSON deserialization SUCCESS");
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[XmlToolbarSerializer.TryDeserializeFromJson<{typeof(T).Name}>] JSON deserialization FAILED: {ex.Message}");
            return null;
        }
    }
}
