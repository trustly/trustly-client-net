using System;
using Newtonsoft.Json;
using Trustly.Api.Domain.Base;

namespace Trustly.Api.Domain.Requests
{
    public class CancelChargeRequestData : AbstractRequestParamsData<EmptyRequestParamsDataAttributes>
    {
        /// <summary>
        /// The OrderID of the Charge request that should be canceled.
        /// </summary>
        public string OrderId { get; set; }
    }

    public class CancelChargeResponseData : AbstractResponseResultData
    {
        /// <summary>
        /// 1 if the Charge could be canceled, and 0 otherwise.
        /// </summary>
        [JsonProperty("result")]
        public bool Result { get; set; }

        /// <summary>
        /// If the CancelCharge was NOT accepted and result 0 is sent,
        /// a textual code describing the rejection reason will be sent here.
        ///
        /// For a successful CancelCharge, this will be null.
        /// </summary>
        [JsonProperty("rejected", NullValueHandling = NullValueHandling.Include)]
        public string Rejected { get; set; }
    }
}
