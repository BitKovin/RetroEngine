using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RetroEngine.Helpers
{

    public static class JsonConverters
    {
        public static List<JsonConverter> GetAll()
        {
            return new List<JsonConverter> {new Vector3Converter(), new TypeConverter() };
        }
    }

    public class TypeConverter : JsonConverter<Type>
    {
        public override Type Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
            )
        {
            string assemblyQualifiedName = reader.GetString();
            return Type.GetType(assemblyQualifiedName);
            //throw new NotSupportedException();
        }

        public override void Write(
            Utf8JsonWriter writer,
            Type value,
            JsonSerializerOptions options
            )
        {
            string assemblyQualifiedName = value.AssemblyQualifiedName;
            // Use this with caution, since you are disclosing type information.
            writer.WriteStringValue(assemblyQualifiedName);
        }
    }

    public class Vector3Converter : JsonConverter<Microsoft.Xna.Framework.Vector3>
{
    public override Microsoft.Xna.Framework.Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        float x = 0, y = 0, z = 0;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return new Microsoft.Xna.Framework.Vector3(x, y, z);

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException();

            string propertyName = reader.GetString();

            reader.Read();

            switch (propertyName)
            {
                case "X":
                    x = reader.GetSingle();
                    break;
                case "Y":
                    y = reader.GetSingle();
                    break;
                case "Z":
                    z = reader.GetSingle();
                    break;
                default:
                    throw new JsonException();
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, Microsoft.Xna.Framework.Vector3 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("X", value.X);
        writer.WriteNumber("Y", value.Y);
        writer.WriteNumber("Z", value.Z);
        writer.WriteEndObject();
    }
}
}
