using System;
using Newtonsoft.Json;
using Trustly.Api.Domain.Base;
using Trustly.Api.Domain.Common;

namespace Trustly.Api.Domain.Requests
{
    public class ChargeRequestData : AbstractRequestParamsData<ChargeRequestDataAttributes>
    {
        /// <summary>
        /// The AccountID received from an account notification which shall be charged.
        /// </summary>
        public string AccountID { get; set; }

        /// <summary>
        /// The URL to which notifications for this payment should be sent to.This URL should be hard to guess and not contain a? ("question mark").
        /// </summary>
        public string NotificationURL { get; set; }

        /// <summary>
        /// ID, username, hash or anything uniquely identifying the end-user being charged.
        ///
        /// Preferably the same ID/username as used in the merchant's own backoffice in order to simplify for the merchant's support department.
        /// </summary>
        public string EndUserID { get; set; }

        /// <summary>
        /// Your unique ID for the charge.
        /// </summary>
        public string MessageID { get; set; }

        /// <summary>
        /// The amount to charge with exactly two decimals.Only digits. Use dot (.) as decimal separator.
        /// </summary>
        public string Amount { get; set; }

        /// <summary>
        /// The currency of the amount to charge.
        /// </summary>
        public string Currency { get; set; }
    }

    public class ChargeResponseData : AbstractResponseResultData
    {
        /// <summary>
        /// 1 if the charge was accepted for processing, 0 otherwise. Note that this is an acceptance of the order, no money has been charged from the account until you receive notifications thereof.
        /// </summary>
        [JsonProperty("result")]
        public bool Result { get; set; }

        /// <summary>
        /// The globally unique OrderID the charge order was assigned in our system, or null if the charge was not accepted.
        /// The order has no end-user interaction; it is merely used as a reference for the notifications delivered regarding the charge. See section "Notifications" below for details.
        /// </summary>
        [JsonProperty("orderid")]
        public string OrderID { get; set; }

        /// <summary>
        /// If the charge was NOT accepted, a textual code describing the rejection reason, null otherwise.
        ///
        /// The possible rejected codes are:
        /// 
        /// ERROR_MANDATE_NOT_FOUND - the AccountID does not have an active mandate
        /// ERROR_DIRECT_DEBIT_NOT_ALLOWED - Trustly Direct Debit is not enabled on the merchant's account in Trustly's system.
        /// ERROR_ACCOUNT_NOT_FOUND - the specified AccountID does not exist.
        /// </summary>
        [JsonProperty("rejected")]
        public string Rejected { get; set; }
    }

    public class ChargeRequestDataAttributes : AbstractRequestParamsDataAttributes
    {
        /// <summary>
        /// The text to show on the end-user's bank statement as well as in end-user e-mail communication.
        /// On the bank statement, only the first seven characters (along with Trustly's reference) will be shown.
        /// Allowed characters: A-Z(both upper and lower case), 0-9, ".", "-", "_", " " (dot, dash, underscore, space).
        /// </summary>
        /// 
        /// <example>
        /// Shopperstatement: "Sport Shop". Will result in "T Sport S xyz" in bank statement and "Sport Shop" in e-mails.
        /// </example>
        public string ShopperStatement { get; set; }

        /// <summary>
        /// The email address of the end user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The date when the funds will be charged from the end user's bank account. If this attribute is not sent, the charge will be attempted as soon as possible.
        /// </summary>
        public string PaymentDate { get; set; }

        /// <summary>
        /// The ExternalReference is a reference set by the merchant for any purpose and does not need to be unique for every API call.
        /// For example, it can be used for invoice references, OCR numbers and also for offering end users the option to part-pay an invoice using the same ExternalReference.
        /// The ExternalReference will be included in version 1.2 of the settlement report, ViewAutomaticSettlementDetailsCSV.
        /// </summary>
        /// <example>32423534523</example>
        public string ExternalReference { get; set; }
    }
}
