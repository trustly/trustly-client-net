
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Trustly.Api.Domain
{
    public class WrapperConverter<T> : JsonConverter where T : class
    {
        private readonly string propertyName;

        public WrapperConverter(string propertyName)
        {
            this.propertyName = propertyName;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(T);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JToken.Load(reader);
            var constructor = typeof(T).GetConstructor(new[] { typeof(JToken) });
            if (constructor == null)
            {
                throw new JsonSerializationException($"No 'new(JToken)' constructor found for type {typeof(T)}");
            }

            return constructor.Invoke(new object[] { jsonObject }) as T;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var dataProperty = value.GetType().GetProperty(this.propertyName);
            if (dataProperty == null)
            {
                throw new JsonSerializationException($"No '{this.propertyName}' property found on type {value.GetType()}");
            }
            
            serializer.Serialize(writer, dataProperty.GetValue(value));
        }
    }
}
