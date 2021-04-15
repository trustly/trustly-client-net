using System;
using Trustly.Api.Domain.Base;

namespace Trustly.Api.Domain.Requests
{
    public class AccountLedgerRequestData : AbstractRequestParamsData<EmptyRequestParamsDataAttributes>
    {
        public string FromDate { get; set; }

        public string ToDate { get; set; }

        public string Currency { get; set; }
    }
}
