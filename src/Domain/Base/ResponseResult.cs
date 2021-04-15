using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Trustly.Api.Domain.Base
{
    public class ResponseResult<TData>
        where TData : AbstractResponseResultData
    {
        [JsonProperty("signature")]
        public string Signature { get; set; }

        [JsonProperty("uuid")]
        public string UUID { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("data")]
        public TData Data { get; set; }
    }
}
