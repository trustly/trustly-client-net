using System;
using Newtonsoft.Json;
using Trustly.Api.Domain.Base;

namespace Trustly.Api.Domain.Notifications
{
    public class AbstractCreditDebitPendingNotificationData : AbstractRequestParamsData<EmptyRequestParamsDataAttributes>
    {
        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("messageid")]
        public string MessageID { get; set; }

        [JsonProperty("orderid")]
        public string OrderID { get; set; }

        [JsonProperty("enduserid")]
        public string EnduserID { get; set; }

        [JsonProperty("notificationid")]
        public string NotificationID { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }
    }
}
