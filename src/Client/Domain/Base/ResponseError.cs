using System;
using Newtonsoft.Json;

namespace Trustly.Api.Domain.Base
{
    public class ResponseError : ResponseErrorData
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("error")]
        public ResponseResult<ResponseErrorData> Error { get; set; }
    }
}
