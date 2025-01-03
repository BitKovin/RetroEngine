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
            return new List<JsonConverter> { new Vector3Converter(), new TypeConverter(), new SystemVector3Converter(), new MatrixConverter(), new DelayConverter() };
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

    public class SystemVector3Converter : JsonConverter<System.Numerics.Vector3>
    {
        public override System.Numerics.Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            float x = 0, y = 0, z = 0;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return new System.Numerics.Vector3(x, y, z);

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

        public override void Write(Utf8JsonWriter writer, System.Numerics.Vector3 value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("X", value.X);
            writer.WriteNumber("Y", value.Y);
            writer.WriteNumber("Z", value.Z);
            writer.WriteEndObject();
        }
    }

    public class DelayConverter : JsonConverter<Delay>
    {
        public override Delay Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            float x = 0;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    Delay d = new Delay();
                    d.AddDelay(x);
                    return d;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException();

                string propertyName = reader.GetString();

                reader.Read();

                switch (propertyName)
                {
                    case "T":
                        x = reader.GetSingle();
                        break;
                    default:
                        throw new JsonException();
                }
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, Delay value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("T", value.GetRemainTime());
            writer.WriteEndObject();
        }
    }

    public class MatrixConverter : JsonConverter<Microsoft.Xna.Framework.Matrix>
    {
        public override Microsoft.Xna.Framework.Matrix Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            float m11 = 0, m12 = 0, m13 = 0, m14 = 0;
            float m21 = 0, m22 = 0, m23 = 0, m24 = 0;
            float m31 = 0, m32 = 0, m33 = 0, m34 = 0;
            float m41 = 0, m42 = 0, m43 = 0, m44 = 0;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return new Microsoft.Xna.Framework.Matrix(
                        m11, m12, m13, m14,
                        m21, m22, m23, m24,
                        m31, m32, m33, m34,
                        m41, m42, m43, m44
                    );
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException();

                string propertyName = reader.GetString();

                reader.Read();

                switch (propertyName)
                {
                    case "M11":
                        m11 = reader.GetSingle();
                        break;
                    case "M12":
                        m12 = reader.GetSingle();
                        break;
                    case "M13":
                        m13 = reader.GetSingle();
                        break;
                    case "M14":
                        m14 = reader.GetSingle();
                        break;
                    case "M21":
                        m21 = reader.GetSingle();
                        break;
                    case "M22":
                        m22 = reader.GetSingle();
                        break;
                    case "M23":
                        m23 = reader.GetSingle();
                        break;
                    case "M24":
                        m24 = reader.GetSingle();
                        break;
                    case "M31":
                        m31 = reader.GetSingle();
                        break;
                    case "M32":
                        m32 = reader.GetSingle();
                        break;
                    case "M33":
                        m33 = reader.GetSingle();
                        break;
                    case "M34":
                        m34 = reader.GetSingle();
                        break;
                    case "M41":
                        m41 = reader.GetSingle();
                        break;
                    case "M42":
                        m42 = reader.GetSingle();
                        break;
                    case "M43":
                        m43 = reader.GetSingle();
                        break;
                    case "M44":
                        m44 = reader.GetSingle();
                        break;
                    default:
                        throw new JsonException();
                }
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, Microsoft.Xna.Framework.Matrix value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("M11", value.M11);
            writer.WriteNumber("M12", value.M12);
            writer.WriteNumber("M13", value.M13);
            writer.WriteNumber("M14", value.M14);
            writer.WriteNumber("M21", value.M21);
            writer.WriteNumber("M22", value.M22);
            writer.WriteNumber("M23", value.M23);
            writer.WriteNumber("M24", value.M24);
            writer.WriteNumber("M31", value.M31);
            writer.WriteNumber("M32", value.M32);
            writer.WriteNumber("M33", value.M33);
            writer.WriteNumber("M34", value.M34);
            writer.WriteNumber("M41", value.M41);
            writer.WriteNumber("M42", value.M42);
            writer.WriteNumber("M43", value.M43);
            writer.WriteNumber("M44", value.M44);
            writer.WriteEndObject();
        }
    }


}
