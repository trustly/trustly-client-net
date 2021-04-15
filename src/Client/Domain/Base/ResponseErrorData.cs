using System;
using Newtonsoft.Json;

namespace Trustly.Api.Domain.Base
{
    public class ResponseErrorData : AbstractResponseResultData
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
