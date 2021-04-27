using System;
using Newtonsoft.Json;

namespace Trustly.Api.Domain.Common
{

    public class StringBooleanJsonConverter : JsonConverter<bool>
    {
        public override bool ReadJson(JsonReader reader, Type objectType, bool existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var valueString = (reader.Value ?? "").ToString();
            if (string.Equals("1", valueString, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        public override void WriteJson(JsonWriter writer, bool value, JsonSerializer serializer)
        {
            writer.WriteValue(value ? "1" : "0");
        }
    }
}
