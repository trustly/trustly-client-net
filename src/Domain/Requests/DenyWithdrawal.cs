using System;
using Newtonsoft.Json;
using Trustly.Api.Domain.Base;
using Trustly.Api.Domain.Common;

namespace Trustly.Api.Domain.Requests
{
    public class DenyWithdrawalRequestData : AbstractToTrustlyRequestParamsData<EmptyRequestParamsDataAttributes>
    {
        public long OrderID { get; set; }
    }

    public class DenyWithdrawalResponseData : AbstractResponseResultData
    {
        /// <summary>
        /// The OrderID specified when calling the method.
        /// </summary>
        [JsonProperty("orderid")]
        public long OrderID { get; set; }

        /// <summary>
        /// "1" if the refund request is accepted by Trustly's system.
        /// If the refund request is not accepted, you will get an error code back in the <see cref="JsonRpcResponse{TData}.Error"/>
        /// </summary>
        [JsonProperty("result")]
        [JsonConverter(typeof(StringBooleanJsonConverter))]
        public bool Result { get; set; }
    }
}
