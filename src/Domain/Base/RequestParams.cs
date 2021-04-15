using System;
using Newtonsoft.Json;

namespace Trustly.Api.Domain.Base
{
    public class RequestParams<TParamsData>
        where TParamsData : AbstractRequestParamsData
    {
        [JsonProperty("Signature")]
        public string Signature { get; set; }

        [JsonProperty("UUID")]
        public string UUID { get; set; }

        [JsonProperty("Data")]
        public TParamsData Data { get; set; }
    }
}
