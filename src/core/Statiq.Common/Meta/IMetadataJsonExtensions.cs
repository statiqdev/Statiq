using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;

namespace Statiq.Common
{
    public static class IMetadataJsonExtensions
    {
        public static string ToJson(this IMetadata metadata, JsonSerializerOptions options = null) =>
            JsonSerializer.Serialize<IReadOnlyDictionary<string, object>>(metadata, options);

        public static string ToJson(this IEnumerable<IMetadata> metadata, JsonSerializerOptions options = null) =>
            JsonSerializer.Serialize(metadata?.Cast<IReadOnlyDictionary<string, object>>().ToArray(), options);
    }
}
