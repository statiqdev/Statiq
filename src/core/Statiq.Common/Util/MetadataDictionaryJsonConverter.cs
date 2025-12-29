using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Statiq.Common;

namespace Statiq.Common
{
    /// <summary>
    /// A JSON converter that will convert a JSON object to nested <see cref="IMetadata"/>.
    /// </summary>
    public class MetadataDictionaryJsonConverter : JsonConverter<MetadataDictionary>
    {
        public override MetadataDictionary Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Only JSON objects can be converted to metadata");
            }
            return ReadObject(ref reader);
        }

        private static MetadataDictionary ReadObject(ref Utf8JsonReader reader)
        {
            MetadataDictionary metadata = new MetadataDictionary();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return metadata;
                }

                ReadProperty(ref reader, metadata);
            }
            throw new JsonException("Unexpected end of reader while reading JSON object");
        }

        private static void ReadProperty(ref Utf8JsonReader reader, MetadataDictionary metadata)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException($"Expected a {JsonTokenType.PropertyName} token but got a {reader.TokenType.ToString()} token");
            }
            string key = reader.GetString();
            if (!reader.Read())
            {
                throw new JsonException("Expected property value not found");
            }
            metadata[key] = ReadValue(ref reader);
        }

        private static object ReadValue(ref Utf8JsonReader reader)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.True:
                    return true;
                case JsonTokenType.False:
                    return false;
                case JsonTokenType.Number:
                    if (reader.TryGetInt32(out int i))
                    {
                        return i;
                    }
                    if (reader.TryGetInt64(out long l))
                    {
                        return l;
                    }
                    return reader.GetDouble();
                case JsonTokenType.StartArray:
                    return ReadArray(ref reader);
                case JsonTokenType.StartObject:
                    return ReadObject(ref reader);
            }

            // Return the string representation if all else fails
            return reader.GetString();
        }

        private static object ReadArray(ref Utf8JsonReader reader)
        {
            List<object> array = new List<object>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    return array.ToArray();
                }
                array.Add(ReadValue(ref reader));
            }
            throw new JsonException("Unexpected end of reader while reading JSON array");
        }

        public override void Write(Utf8JsonWriter writer, MetadataDictionary value, JsonSerializerOptions options) =>
            throw new NotSupportedException($"The {nameof(MetadataDictionaryJsonConverter)} does not support serializing to JSON.");
    }
}
