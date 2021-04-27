using System;
using System.ComponentModel.DataAnnotations;
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
        [Required]
        [JsonProperty("Username")]
        public string Username { get; set; }

        [Required]
        [JsonProperty("Password")]
        public string Password { get; set; }
    }
}
