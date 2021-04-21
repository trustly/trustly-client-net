using System;
using Newtonsoft.Json;

namespace Trustly.Api.Domain.Base
{
    public class RequestParams<TData>
        where TData : IRequestParamsData
    {
        [JsonProperty("Signature")]
        public string Signature { get; set; }

        [JsonProperty("UUID")]
        public string UUID { get; set; }

        [JsonProperty("Data")]
        public TData Data { get; set; }
    }
}
