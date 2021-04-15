using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Trustly.Api.Domain.Base
{
    public abstract class AbstractResponseResultData
    {
        [JsonExtensionData]
        public Dictionary<string, object> ExtensionData { get; set; }
    }
}
