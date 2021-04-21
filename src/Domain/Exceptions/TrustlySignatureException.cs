using System;

namespace Trustly.Api.Domain.Exceptions
{
    public class TrustlySignatureException : AbstractTrustlyApiException
    {
        public TrustlySignatureException(string message) : base(message) { }
        public TrustlySignatureException(string message, Exception cause) : base(message, cause) { }
    }
}
