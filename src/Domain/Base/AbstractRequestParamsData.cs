using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Trustly.Api.Domain.Base
{
    /// <summary>
    /// Marker interface for restricting the generics through the project.
    /// We can use it to say that we want request params data, but not at all of what type.
    /// </summary>
    public interface IRequestParamsData : IData
    {

    }

    public abstract class AbstractRequestParamsData<TAttr> : IRequestParamsData
        where TAttr : AbstractRequestParamsDataAttributes
    {
        //[System.ComponentModel.DataAnnotations.Va]
        public TAttr Attributes { get; set; }
    }
}
