using System;

namespace Trustly.Api.Domain.Exceptions
{
    public abstract class AbstractTrustlyApiException : Exception
    {
        public AbstractTrustlyApiException(string message) : base(message) { }
        public AbstractTrustlyApiException(string message, Exception cause) : base(message, cause) { }

        public JsonRpcError ResponseError { get; set; }
    }
}
