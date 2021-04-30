using System;
using Trustly.Api.Domain.Base;

namespace Trustly.Api.Client
{
    public class NotificationArgs<TData>
        where TData : IRequestParamsData
    {
        public TData Data { get; }
        public Action RespondWithOK { get; }
        public Action RespondWithError { get; }

        public NotificationArgs(TData data, Action respondWithOK, Action respondWithError)
        {
            this.Data = data;
            this.RespondWithOK = respondWithOK;
            this.RespondWithError = respondWithError;
        }
    }
}
