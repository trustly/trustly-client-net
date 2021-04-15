using System;
using Newtonsoft.Json;

namespace Trustly.Api.Domain.Base
{
    public class RequestParams<TData, TDataAttr>
        where TData : AbstractRequestParamsData<TDataAttr>
        where TDataAttr : AbstractRequestParamsDataAttributes
    {
        [JsonProperty("Signature")]
        public string Signature { get; set; }

        [JsonProperty("UUID")]
        public string UUID { get; set; }

        [JsonProperty("Data")]
        public TData Data { get; set; }
    }
}
