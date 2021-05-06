using System;

namespace Trustly.Api.Domain.Exceptions
{
    public class TrustlyRejectionException : Exception
    {
        public TrustlyRejectionException(string message) : base(message) { }
        public TrustlyRejectionException(string message, Exception cause) : base(message, cause) { }

        public string Reason { get; set; }
    }
}
