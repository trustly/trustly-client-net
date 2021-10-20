using System;
using Newtonsoft.Json;

namespace Trustly.Api.Domain.Base
{
    public class JsonRpcResponse<TData>
        where TData : IResponseResultData
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

        public IData GetData()
        {
            if (this.IsSuccessfulResult())
            {
                return this.Result.Data;
            }
            else
            {
                if (this.Error != null && this.Error.Error != null)
                {
                    return this.Error.Error.Data;
                }
            }

            return null;
        }

        public string GetMethod()
        {
            return IsSuccessfulResult() ? Result?.Method : Error?.Error?.Method;
        }

        public string GetSignature()
        {
            return IsSuccessfulResult() ? Result?.Signature : Error?.Error?.Signature;
        }
    }
}
