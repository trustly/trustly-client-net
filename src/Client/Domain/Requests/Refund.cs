using System;
using Newtonsoft.Json;
using Trustly.Api.Domain.Base;

namespace Trustly.Api.Domain.Requests
{
    public class RefundRequestData : AbstractRequestParamsData<RefundRequestDataAttributes>
    {
        public string OrderID { get; set; }

        public string Amount { get; set; }

        public string Currency { get; set; }
    }

    public class RefundResponseData : AbstractResponseResultData
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
        public long Result { get; set; }
    }

    public class RefundRequestDataAttributes : AbstractRequestParamsDataAttributes
    {
        /// <summary>
        /// The ExternalReference is a reference set by the merchant for any purpose and does not need to be unique for every API call.
        /// The ExternalReference will be included in version 1.2 of the settlement report, ViewAutomaticSettlementDetailsCSV.
        /// </summary>
        public string ExternalReference { get; set; }
    }
}
