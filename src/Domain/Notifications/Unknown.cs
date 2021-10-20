using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Trustly.Api.Domain.Base;

namespace Trustly.Api.Domain.Notifications
{
    public class UnknownNotificationData : AbstractFromTrustlyRequestParamsData<EmptyRequestParamsDataAttributes>
    {
        [JsonExtensionData]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> ExtensionData { get; set; }
    }
}
