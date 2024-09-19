using System;
using Trustly.Api.Domain;

namespace Trustly.Api.Client
{
    public class JsonRpcFactory
    {
        public JsonRpcRequest<TReqAttr, TReqData, JsonRpcRequestParams<TReqAttr, TReqData>> Create<TReqAttr, TReqData>(TReqData requestData, string method, string uuid = null)
            where TReqAttr : AbstractRequestDataAttributes
            where TReqData : AbstractRequestData<TReqAttr>
        {
            return new JsonRpcRequest<TReqAttr, TReqData, JsonRpcRequestParams<TReqAttr, TReqData>>(method)
            {
                Params = new JsonRpcRequestParams<TReqAttr, TReqData>
                {
                    UUID = uuid ?? Guid.NewGuid().ToString(),
                    Data = requestData
                }
            };
        }


        public JsonRpcResponse<TAckData, ResponseResult<TAckData>> CreateResponse<TReqData, TAckData>(
            JsonRpcNotification<TReqData, JsonRpcNotificationParams<TReqData>> request,
            TAckData data
        )
        {
            return new JsonRpcResponse<TAckData, ResponseResult<TAckData>>()
            {
                Result = new ResponseResult<TAckData>
                {
                    Method = request.Method,
                    Data = data,
                    UUID = request.Params.UUID
                }
            };
        }

        public JsonRpcResponse<TAckData, ResponseResult<TAckData>> CreateResponse<TAckData>(
            TAckData data,
            string method,
            string uuid
        )
        {
            return new JsonRpcResponse<TAckData, ResponseResult<TAckData>>()
            {
                Result = new ResponseResult<TAckData>
                {
                    Method = method,
                    Data = data,
                    UUID = uuid
                }
            };
        }
    }
}
