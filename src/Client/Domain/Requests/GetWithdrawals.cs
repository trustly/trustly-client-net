using System;
using Trustly.Api.Domain.Base;

namespace Trustly.Api.Domain.Requests
{
    public class GetWithdrawalsRequestData : AbstractRequestParamsData<EmptyRequestParamsDataAttributes>
    {
        public string OrderID { get; set; }
    }
}
