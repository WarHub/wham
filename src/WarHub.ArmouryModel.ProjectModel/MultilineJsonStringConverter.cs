using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WarHub.ArmouryModel.ProjectModel
{
    /// <summary>
    /// Converts multiline strings into arrays of strings.
    /// </summary>
    internal class MultilineJsonStringConverter : JsonConverter
    {
        private const char LF = '\n';
        private const string StringLF = "\n";

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null || (reader.TokenType != JsonToken.StartArray && reader.TokenType != JsonToken.String))
            {
                return null;
            }
            if (reader.TokenType == JsonToken.String)
            {
                return reader.Value.ToString();
            }
            ReadUntilStringOrEndArray();
            if (reader.TokenType != JsonToken.String)
            {
                return "";
            }
            var firstLine = reader.Value.ToString();
            ReadUntilStringOrEndArray();
            if (reader.TokenType != JsonToken.String)
            {
                return firstLine;
            }
            var lines = new List<string>
                {
                    firstLine
                };
            do
            {
                lines.Add(reader.Value.ToString());
                ReadUntilStringOrEndArray();
            }
            while (reader.TokenType == JsonToken.String);
            return string.Join(StringLF, lines);
            void ReadUntilStringOrEndArray()
            {
                while (reader.Read() && reader.TokenType != JsonToken.String && reader.TokenType != JsonToken.EndArray)
                {
                }
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var text = value.ToString();
            // CA1307 here is invalid - IndexOf is by default doing Ordinary comparison
#pragma warning disable CA1307 // Specify StringComparison
            if (text.IndexOf(LF) < 0)
#pragma warning restore CA1307 // Specify StringComparison
            {
                writer.WriteValue(text);
                return;
            }
            writer.WriteStartArray();
            var lines = text.Split(LF);
            foreach (var line in lines)
            {
                writer.WriteValue(line);
            }
            writer.WriteEndArray();
        }
    }
}
