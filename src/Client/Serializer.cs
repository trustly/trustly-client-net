using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Trustly.Api.Domain.Base;

namespace Trustly.Api.Client
{
    public class Serializer
    {
        public string SerializeData<TData>(TData data)
            where TData : IData
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            var jsonString = JsonConvert.SerializeObject(data, settings);

            var jsonObject = JToken.Parse(jsonString);
            var sb = new StringBuilder();

            this.SerializeToken(jsonObject, sb);

            return sb.ToString();
        }

        private void SerializeToken(JToken token, StringBuilder sb)
        {
            if (token is JObject obj)
            {
                var orderedProperties = obj.Properties()
                    .OrderBy(p => p.Name, StringComparer.InvariantCultureIgnoreCase);

                foreach (var property in orderedProperties)
                {
                    sb.Append(property.Name);
                    this.SerializeToken(property, sb);
                }

            }
            else if (token is JValue value)
            {
                sb.Append(value.Value<string>());
            }
            else
            {
                foreach (var child in token.Children())
                {
                    this.SerializeToken(child, sb);
                }
            }
        }
    }
}
