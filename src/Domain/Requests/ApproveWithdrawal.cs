using System;
using Newtonsoft.Json;
using Trustly.Api.Domain.Base;

namespace Trustly.Api.Domain.Requests
{
    public class ApproveWithdrawalRequestData : AbstractToTrustlyRequestParamsData<EmptyRequestParamsDataAttributes>
    {
        public uint OrderID { get; set; }
    }

    public class ApproveWithdrawalResponseData : AbstractResponseResultData
    {
        /// <summary>
        /// The OrderID specified when calling the method.
        /// </summary>
        [JsonProperty("orderid")]
        public long OrderID { get; set; }

        /// <summary>
        /// 1 if the withdrawal could be approved and 0 otherwise.
        /// </summary>
        [JsonProperty("result")]
        public bool Result { get; set; }
    }
}
