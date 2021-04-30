using System;

namespace Trustly.Api.Domain.Exceptions
{
    public class TrustlyNoNotificationListenerException : AbstractTrustlyApiException
    {
        public TrustlyNoNotificationListenerException(string message) : base(message) { }
        public TrustlyNoNotificationListenerException(string message, Exception cause) : base(message, cause) { }
    }
}
