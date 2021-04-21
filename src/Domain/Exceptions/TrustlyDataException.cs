using System;

namespace Trustly.Api.Domain.Exceptions
{
    public class TrustlyDataException : AbstractTrustlyApiException
    {
        public TrustlyDataException(string message) : base(message) { }
        public TrustlyDataException(string message, Exception cause) : base(message, cause) { }
    }
}
