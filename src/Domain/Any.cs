
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Trustly.Api.Domain
{
    public class Any
    {
        [JsonExtensionData]
        public JObject AdditionalProperties { get; set; }
    }
}
