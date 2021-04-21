using System;
using Trustly.Api.Domain.Base;

namespace Trustly.Api.Client
{
    public class JsonRpcFactory
    {
        public JsonRpcRequest<TReqData> Create<TReqData>(TReqData requestData, string method, string uuid = null)
            where TReqData : IRequestParamsData
        {
            return new JsonRpcRequest<TReqData>
            {
                Method = method,
                Version = 1.1,
                Params = new RequestParams<TReqData>
                {
                    UUID = uuid ?? Guid.NewGuid().ToString(),
                    Data = requestData
                }
            };
        }
    }
}
