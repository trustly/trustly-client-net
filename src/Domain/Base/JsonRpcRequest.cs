using System;
using Newtonsoft.Json;

namespace Trustly.Api.Domain.Base
{
    public class JsonRpcRequest<TParamsData, TParamsDataAttr>
        where TParamsData : AbstractRequestParamsData<TParamsDataAttr>
        where TParamsDataAttr : AbstractRequestParamsDataAttributes
    {
        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("params")]
        public RequestParams<TParamsData, TParamsDataAttr> Params { get; set; }

        private double? _version;

        [JsonProperty("version")]
        public double Version
        {
            get
            {
                return _version ?? 1.1;
            }
            set
            {
                _version = value;
            }
        }
    }
}
