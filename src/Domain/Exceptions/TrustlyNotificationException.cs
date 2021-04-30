using System;

namespace Trustly.Api.Domain.Exceptions
{
    public class TrustlyNotificationException : AbstractTrustlyApiException
    {
        public TrustlyNotificationException(string message) : base(message) { }
        public TrustlyNotificationException(string message, Exception cause) : base(message, cause) { }
    }
}
