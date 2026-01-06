using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.Enums
{
    public sealed class KebabCaseEnumConverter<T> : JsonConverter<T>
        where T : struct, Enum
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (value == null)
                throw new JsonException();

            var pascal = string.Concat(
                value.Split('-').Select(p => char.ToUpperInvariant(p[0]) + p[1..])
            );

            return Enum.Parse<T>(pascal);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var kebab = string.Concat(
                value.ToString()
                     .Select((c, i) =>
                         char.IsUpper(c) && i > 0 ? "-" + char.ToLowerInvariant(c) : char.ToLowerInvariant(c).ToString()
                     )
            );

            writer.WriteStringValue(kebab);
        }
    }
}
