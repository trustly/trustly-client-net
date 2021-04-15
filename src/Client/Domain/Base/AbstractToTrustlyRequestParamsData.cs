using System;
using Newtonsoft.Json;

namespace Trustly.Api.Domain.Base
{
    public class AbstractToTrustlyRequestParamsData : AbstractRequestParamsData
    {
        [JsonProperty("Username")]
        public string Username { get; set; }

        [JsonProperty("Password")]
        public string Password { get; set; }
    }
}
