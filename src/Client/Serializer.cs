using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Trustly.Api.Client
{
    public class Serializer
    {
        public string SerializeData<TData>(TData data, bool silent = false)
        {
            JObject jsonObject;
            if (data is JToken token)
            {
                // If the value to serialize is already a JToken, then we will assume it is an object.
                // We can also work on the actual exact response, and not rely on flaky JSON -> DTO -> JSON -> String conversion.
                jsonObject = (JObject) token;
            }
            else
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                };

                if (silent)
                {
                    // We ignore any error; useful for test cases or other uncertain scenarios.
                    settings.Error = (sender, e) =>
                    {
                        e.ErrorContext.Handled = true;
                    };
                }

                var jsonSerializer = JsonSerializer.Create(settings);
                jsonObject = JObject.FromObject(data, jsonSerializer);
            }

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
                sb.Append(value.Value<string>());
            }
            else if (token is JProperty property)
            {
                var newPath = new string[propertyPath.Length + 1];
                propertyPath.CopyTo(newPath, 0);
                newPath[newPath.Length - 1] = property.Name;

                if (property.Value.Type == JTokenType.Null)
                {
                    sb.Append(property.Name);
                }
                else
                {
                    var propertyBuffer = new StringBuilder();
                    if (this.SerializeToken(property.Value, propertyBuffer, newPath))
                    {
                        sb.Append(property.Name);
                        sb.Append(propertyBuffer);
                    }
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
