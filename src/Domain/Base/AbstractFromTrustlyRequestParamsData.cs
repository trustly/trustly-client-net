using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Trustly.Api.Domain.Base
{
    public interface IFromTrustlyRequestParamsData : IRequestParamsData
    {
    }

    public class AbstractFromTrustlyRequestParamsData<TAttr> : AbstractRequestParamsData<TAttr>, IFromTrustlyRequestParamsData
        where TAttr : AbstractRequestParamsDataAttributes
    {
        [JsonProperty(PropertyName = "attributes", NullValueHandling = NullValueHandling.Ignore)]
        public TAttr Attributes { get; set; }
    }
}
