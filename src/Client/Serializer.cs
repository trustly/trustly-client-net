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
                NullValueHandling = NullValueHandling.Include
            };

            var jsonString = JsonConvert.SerializeObject(data, settings);

            var jsonObject = JToken.Parse(jsonString);
            var sb = new StringBuilder();

            this.SerializeToken(jsonObject, sb, new string[0]);

            return sb.ToString();
        }

        private bool SerializeToken(JToken token, StringBuilder sb, string[] propertyPath)
        {
            if (token is JObject obj)
            {
                var orderedProperties = obj.Properties()
                    .OrderBy(p => p.Name, StringComparer.InvariantCultureIgnoreCase);

                foreach (var property in orderedProperties)
                {
                    this.SerializeToken(property, sb, propertyPath);
                }
            }
            else if (token is JValue value)
            {
                if (propertyPath[0].ToLower().Equals("attributes"))
                {
                    if (value.Value == null)
                    {
                        // NOTE: Special consideration is made for 'attributes' properties.
                        // Documentation says that <null> should be regarded as <empty string>
                        // But it does not specify that a not included 'attribute' property
                        // is that same as it not existing at all.
                        // This is contrary to how 'data' properties work, since they are typesafe.
                        // But 'attributes' properties were not typesafe, just a map, in older code.
                        // This discrepancy shows its' head here, since this code is typesafe.
                        return false;
                    }
                }

                //sb.Append(propertyPath[propertyPath.Length - 1]);
                sb.Append(value.Value<string>());
            }
            else if (token is JProperty property)
            {
                var newPath = new string[propertyPath.Length + 1];
                propertyPath.CopyTo(newPath, 0);
                newPath[newPath.Length - 1] = property.Name;

                var propertyBuffer = new StringBuilder();
                if (this.SerializeToken(property.Value, propertyBuffer, newPath))
                {
                    sb.Append(property.Name);
                    sb.Append(propertyBuffer);
                }
            }
            else
            {
                foreach (var child in token.Children())
                {
                    this.SerializeToken(child, sb, propertyPath);
                }
            }

            return true;
        }
    }
}
