using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Trustly.Api.Domain.Base
{
    public abstract class AbstractRequestParamsData<TAttr>
        where TAttr : AbstractRequestParamsDataAttributes
    {
        public TAttr Attributes { get; set; }
    }
}
