using System;
using Trustly.Api.Domain.Base;
using System.Threading.Tasks;

namespace Trustly.Api.Client
{
    public delegate Task NotificationResponseDelegate(string method, string uuid);

    public delegate Task NotificationFailResponseDelegate(string method, string uuid, string message);

    public delegate Task NotificationCustomResponseDelegate(string method, string uuid, string status, string message);

    public class NotificationArgs<TData>
        where TData : IRequestParamsData
    {
        public TData Data { get; }

        private readonly string _method;
        private readonly string _uuid;

        private readonly NotificationResponseDelegate _onOK;
        private readonly NotificationFailResponseDelegate _onFailed;
        private readonly NotificationCustomResponseDelegate _onCustomStatus;

        public NotificationArgs(TData data, string method, string uuid, NotificationResponseDelegate onOK, NotificationFailResponseDelegate onFailed,
            NotificationCustomResponseDelegate onCustomStatus)
        {
            this.Data = data;

            this._method = method;
            this._uuid = uuid;

            this._onOK = onOK;
            this._onFailed = onFailed;
            this._onCustomStatus = onCustomStatus;
        }

        public void RespondWithOK()
        {
            this._onOK(this._method, this._uuid);
        }

        public void RespondWithFailed(string message)
        {
            this._onFailed(this._method, this._uuid, message);
        }

        public void RespondWithCustomStatus(string status, string message)
        {
            this._onCustomStatus(this._method, this._uuid, status, message);
        }
    }
}
