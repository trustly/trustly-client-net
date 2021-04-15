using System;
using Newtonsoft.Json;

namespace Trustly.Api.Domain.Base
{
    public class JsonRpcResponse<TData>
        where TData : AbstractResponseResultData
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("result")]
        public ResponseResult<TData> Result { get; set; }

        [JsonProperty("error")]
        public ResponseError Error { get; set; }

        public bool IsSuccessfulResult()
        {
            return Result != null && Error == null;
        }

        public string GetUUID()
        {
            return IsSuccessfulResult() ? Result?.UUID : Error?.Error?.UUID;
        }

        public string GetSignature()
        {
            return IsSuccessfulResult() ? Result?.Signature : Error?.Error?.Signature;
        }
    }
}
