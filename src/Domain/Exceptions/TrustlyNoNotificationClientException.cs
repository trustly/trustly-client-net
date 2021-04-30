using System;

namespace Trustly.Api.Domain.Exceptions
{
    public class TrustlyNoNotificationClientException : AbstractTrustlyApiException
    {
        public TrustlyNoNotificationClientException(string message) : base(message) { }
        public TrustlyNoNotificationClientException(string message, Exception cause) : base(message, cause) { }
    }
}
