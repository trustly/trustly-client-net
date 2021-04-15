using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Trustly.Api.Domain.Base
{
    public abstract class AbstractRequestParamsDataAttributes
    {
        [JsonExtensionData]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> ExtensionData { get; set; }
    }
}
