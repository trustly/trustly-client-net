using System;

namespace Trustly.Api.Domain.Exceptions
{
    public class TrustlyConnectionException : AbstractTrustlyApiException
    {
        public TrustlyConnectionException(string message) : base(message) { }
        public TrustlyConnectionException(string message, Exception cause) : base(message, cause) { }
    }
}
