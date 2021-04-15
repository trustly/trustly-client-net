using System;
using Newtonsoft.Json;

namespace Trustly.Api.Domain.Base
{
    public class AbstractToTrustlyRequestParamsData<TAttr> : AbstractRequestParamsData<TAttr>
        where TAttr : AbstractRequestParamsDataAttributes
    {
        [JsonProperty("Username")]
        public string Username { get; set; }

        [JsonProperty("Password")]
        public string Password { get; set; }
    }
}
