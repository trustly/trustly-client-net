using System;
namespace Trustly.Api.Domain.Base
{
    public interface IWithRejectionResult
    {
        public bool Result { get; set; }
        public string Rejected { get; set; }
    }
}
