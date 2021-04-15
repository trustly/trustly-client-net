using System;
using Newtonsoft.Json;

namespace Trustly.Api.Domain.Base
{
    public class JsonRpcRequest<TData>
        where TData : AbstractRequestParamsData
    {
        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("params")]
        public RequestParams<TData> Params { get; set; }

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
