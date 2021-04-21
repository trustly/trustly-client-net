using System;
using Newtonsoft.Json;
using Trustly.Api.Domain.Base;
using Trustly.Api.Domain.Common;

namespace Trustly.Api.Domain.Requests
{
    public class AccountPayoutRequestData : AbstractToTrustlyRequestParamsData<AccountPayoutRequestDataAttributes>
    {
        /// <summary>
        /// The URL to which notifications for this payment should be sent to. This URL should be hard to guess and not contain a ? ("question mark").
        /// </summary>
        public string NotificationURL { get; set; }

        /// <summary>
        /// The AccountID received from an account notification to which the money shall be sent.
        /// </summary>
        public string AccountID { get; set; }

        /// <summary>
        /// ID, username, hash or anything uniquely identifying the end-user requesting the withdrawal,
        /// Preferably the same ID/username as used in the merchant's own backoffice in order to simplify for the merchant's support department.
        /// </summary>
        public string EndUserID { get; set; }

        /// <summary>
        /// Your unique ID for the payout.
        /// If the MessageID is a previously initiated P2P order then the payout will be attached to that P2P order and the amount must be equal to or lower than the previously deposited amount.
        /// </summary>
        public string MessageID { get; set; }

        /// <summary>
        /// The amount to send with exactly two decimals. Only digits. Use dot (.) as decimal separator.
        /// If the end-user holds a balance in the merchant's system then the amount must have been deducted from that balance before calling this method.
        /// </summary>
        public string Amount { get; set; }

        /// <summary>
        /// The currency of the end-user's account in the merchant's system.
        /// </summary>
        public string Currency { get; set; }
    }

    public class AccountPayoutResponseData : AbstractResponseResultData
    {
        /// <summary>
        /// The globally unique OrderID the account payout order was assigned in our system.
        /// </summary>
        [JsonProperty("orderid")]
        public long OrderID { get; set; }

        /// <summary>
        /// 1 if the payout could be accepted and 0 otherwise.
        /// </summary>
        [JsonProperty("result")]
        public bool Result { get; set; }
    }

    public class AccountPayoutRequestDataAttributes : AbstractRequestParamsDataAttributes
    {
        /// <summary>
        /// The text to show on the end-user's bank statement after Trustly's own 10 digit reference (which always will be displayed first).
        /// The reference must let the end user identify the merchant based on this value. So the ShopperStatement should contain either your brand name, website name, or company name.
        /// 
        /// If possible, try to keep this text as short as possible to maximise the chance that the full reference
        /// will fit into the reference field on the customer's bank since some banks allow only a limited number of characters.
        /// </summary>
        public string ShopperStatement { get; set; }

        /// <summary>
        /// Information about the Payer (ultimate debtor). This is required for some merchants and partners, see below. 
        /// </summary>
        public RecipientOrSenderInformation SenderInformation { get; set; }
    }
}
