using System;
using System.Threading.Tasks;

namespace Trustly.Api.Client
{
    public delegate Task NotificationAckDelegate<TAckData>(TAckData data);
    public delegate Task NotificationRespondDelegate(string stringBody);

    public class NotificationArgs<TNotificationData, TAckData>
    {
        public TNotificationData Data { get; }

        internal string Method { get; private set; }
        internal string UUID { get; private set; }

        internal NotificationAckDelegate<TAckData> Callback { get; private set; }

        public NotificationArgs(TNotificationData data, string method, string uuid, NotificationAckDelegate<TAckData> callback)
        {
            this.Data = data;

            this.Method = method;
            this.UUID = uuid;

            this.Callback = callback;
        }

        public void Respond(TAckData result)
        {
            this.Callback(result);
        }
    }
}
