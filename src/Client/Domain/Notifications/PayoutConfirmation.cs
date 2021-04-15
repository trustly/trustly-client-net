using Newtonsoft.Json;
using Trustly.Api.Domain.Base;

namespace Trustly.Api.Domain.Notifications
{
    /// <summary>
    /// This is a notification that Trustly will send to the merchant's system in order to confirm that a payout has been sent.
    /// 
    /// This notification is not enabled by default. Please contact your Trustly integration manager in case you want to receive it.
    /// </summary>
    /// 
    /// <remarks>
    /// In many cases, the “payoutconfirmation” will be sent within minutes after the AccountPayout request has been received by Trustly.
    /// But in some cases there will be a delay of 1 day or more, since Trustly relies on receiving statement files from banks.
    ///
    /// If you have sent an AccountPayout request and haven’t received a “payoutconfirmation” within an hour or so,
    /// it doesn’t necessarily mean that something is wrong.If you experience this and need to know the actual status of the payout,
    /// you can either use Trustly’s backoffice (Transactions > Withdrawals), or use the GetWithdrawals API method.
    ///
    /// If you use the GetWithdrawals method and receive status EXECUTING or EXECUTED,
    /// it means that the withdrawal is currently being processed or has been processed, but is not confirmed yet.
    ///
    /// The GetWithdrawals method must not be used more than once every 15 minutes per payout.
    ///
    /// Please note that even if a “payoutconfirmation” has been sent, the payout can still fail afterwards.
    /// If that happens, Trustly will send a credit notification to the merchant’s NotificationURL.
    /// This can happen for example if the funds are sent to a bank account that is closed.
    /// </remarks>
    public class PayoutConfirmationNotificationData : AbstractCreditDebitPendingNotificationData
    {
        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("messageid")]
        public string MessageID { get; set; }

        [JsonProperty("orderid")]
        public string OrderID { get; set; }

        [JsonProperty("enduserid")]
        public string EnduserID { get; set; }

        [JsonProperty("notificationid")]
        public string NotificationID { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }
    }
}
