using System;
using Newtonsoft.Json;
using Trustly.Api.Domain.Base;

namespace Trustly.Api.Domain.Notifications
{
    public class NotificationResponse : AbstractResponseResultData
    {
        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
