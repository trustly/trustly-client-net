using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Trustly.Api.Domain.Notifications
{
    public class UnknownNotificationRequest : JsonRpcNotification<AnyAttributes, JsonRpcNotificationParams<AnyAttributes>>
    {
        public UnknownNotificationRequest(JsonRpcNotificationParams<AnyAttributes> @params, string method) : base(method)
        {
            this.Params = @params;
        }
    }
}
