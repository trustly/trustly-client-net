using System;
namespace Trustly.Api.Domain.Base
{
    public interface IWithRejectionResult
    {
        bool Result { get; set; }
        string Rejected { get; set; }
    }
}
