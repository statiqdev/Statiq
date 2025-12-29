using System;
using System.IO;
using Statiq.Common;
using YamlDotNet.Serialization;
using YamlDotNet.Core;

namespace Statiq.Yaml
{
    public static class IFileExtensions
    {
        public static TValue DeserializeYaml<TValue>(this IFile file)
        {
            file.ThrowIfNull(nameof(file));
            Deserializer deserializer = new Deserializer();
            using (Stream stream = file.OpenRead())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    MergingParser parser = new MergingParser(new Parser(reader));
                    return deserializer.Deserialize<TValue>(reader);
                }
            }
        }

        public static object DeserializeYaml(this IFile file, Type returnType)
        {
            file.ThrowIfNull(nameof(file));
            Deserializer deserializer = new Deserializer();
            using (Stream stream = file.OpenRead())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    MergingParser parser = new MergingParser(new Parser(reader));
                    return deserializer.Deserialize(reader, returnType);
                }
            }
        }

        public static void SerializeYaml<TValue>(this IFile file, TValue value, bool createDirectory)
        {
            file.ThrowIfNull(nameof(file));
            Serializer serializer = new Serializer();
            using (Stream stream = file.OpenWrite(createDirectory))
            {
                long initialPosition = stream.Position;
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    serializer.Serialize(writer, value, typeof(TValue));
                }
                long length = stream.Position - initialPosition;
                stream.SetLength(length);
            }
        }

        public static void SerializeYaml<TValue>(this IFile file, TValue value) =>
            file.SerializeYaml<TValue>(value, true);

        public static void SerializeYaml(this IFile file, object value, Type valueType, bool createDirectory)
        {
            file.ThrowIfNull(nameof(file));
            Serializer serializer = new Serializer();
            using (Stream stream = file.OpenWrite(createDirectory))
            {
                long initialPosition = stream.Position;
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    serializer.Serialize(writer, value, valueType);
                }
                long length = stream.Position - initialPosition;
                stream.SetLength(length);
            }
        }

        public static void SerializeYaml(this IFile file, object value, Type valueType) =>
            file.SerializeYaml(value, valueType, true);

        public static void SerializeYaml(this IFile file, object value, bool createDirectory)
        {
            file.ThrowIfNull(nameof(file));
            Serializer serializer = new Serializer();
            using (Stream stream = file.OpenWrite(createDirectory))
            {
                long initialPosition = stream.Position;
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    serializer.Serialize(writer, value);
                }
                long length = stream.Position - initialPosition;
                stream.SetLength(length);
            }
        }

        public static void SerializeYaml(this IFile file, object value) =>
            file.SerializeYaml(value, true);
    }
}
