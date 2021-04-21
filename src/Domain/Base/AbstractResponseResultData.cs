using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Trustly.Api.Domain.Base
{
    public interface IResponseResultData : IData
    {
        Dictionary<string, object> ExtensionData { get; set; }
    }

    public abstract class AbstractResponseResultData : IResponseResultData
    {
        [JsonExtensionData]
        public Dictionary<string, object> ExtensionData { get; set; }
    }
}
