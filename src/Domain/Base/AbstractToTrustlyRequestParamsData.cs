using System;
using Newtonsoft.Json;

namespace Trustly.Api.Domain.Base
{
    public interface IToTrustlyRequestParamsData : IRequestParamsData
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class AbstractToTrustlyRequestParamsData<TAttr> : AbstractRequestParamsData<TAttr>, IToTrustlyRequestParamsData
        where TAttr : AbstractRequestParamsDataAttributes
    {
        [JsonProperty("Username")]
        public string Username { get; set; }

        [JsonProperty("Password")]
        public string Password { get; set; }
    }
}
