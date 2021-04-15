using System;
using Trustly.Api.Domain.Base;

namespace Trustly.Api.Domain.Requests
{
    public class DenyWithdrawalRequestData : AbstractRequestParamsData<EmptyRequestParamsDataAttributes>
    {
        public long OrderID { get; set; }
    }
}
