using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Trustly.Api.Domain.Base;
using Trustly.Api.Domain.Common;

namespace Trustly.Api.Domain.Requests
{
    public class ApproveWithdrawalRequestData : AbstractToTrustlyRequestParamsData<EmptyRequestParamsDataAttributes>
    {
        public long OrderID { get; set; }
    }

    public class ApproveWithdrawalResponseData : AbstractResponseResultData
    {
        /// <summary>
        /// The OrderID specified when calling the method.
        /// </summary>
        [Required]
        [JsonProperty("orderid")]
        public long OrderID { get; set; }

        /// <summary>
        /// 1 if the withdrawal could be approved and 0 otherwise.
        /// </summary>
        [JsonProperty("result")]
        [JsonConverter(typeof(StringBooleanJsonConverter))]
        public bool Result { get; set; }
    }
}
