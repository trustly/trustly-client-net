using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Trustly.Api.Domain.Base;
using Trustly.Api.Domain.Common;

namespace Trustly.Api.Domain.Requests
{
    public class SelectAccountRequestData : AbstractToTrustlyRequestParamsData<SelectAccountRequestDataAttributes>
    {
        /// <summary>
        /// The URL to which notifications for this order should be sent to.This URL should be hard to guess and not contain a? ("question mark").
        /// </summary>
        /// <example>https://example.com/trustly/notification/a2b63j23dj23883jhfhfh</example>
        [Required]
        public string NotificationURL { get; set; }

        /// <summary>
        /// ID, username, hash or anything uniquely identifying the end-user to be identified.
        /// Preferably the same ID/username as used in the merchant's own backoffice in order to simplify for the merchant's support department
        /// </summary>
        [Required]
        public string EndUserID { get; set; }

        /// <summary>
        /// Your unique ID for the account selection order. Each order you create must have an unique MessageID.
        /// </summary>
        [Required]
        public string MessageID { get; set; }
    }

    public class SelectAccountResponseData : AbstractResponseResultData
    {
        /// <summary>
        /// The globally unique OrderID the account selection order was assigned in our system.
        /// </summary>
        /// <example>7653345737</example>
        [JsonProperty("orderid")]
        public string OrderID { get; set; }

        /// <summary>
        /// The URL that should be loaded so that the end-user can complete the identification process.
        /// </summary>
        /// <example>https://trustly.com/_/2f6b14fa-446a-4364-92f8-84b738d589ff</example>
        [JsonProperty("url")]
        public string URL { get; set; }
    }

    public class SelectAccountRequestDataAttributes : AbstractDepositAndWithdrawAndSelectAccountDataAttributes
    {
        /// <summary>
        /// Only for Trustly Direct Debit.
        /// Request a direct debit mandate from the selected account. 1 or 0. See section "Direct Debit Mandates" below for details.
        ///
        /// If this is set to 1, then <seealso cref="AbstractDepositAndWithdrawAndSelectAccountDataAttributes.Email"/> is required.
        /// </summary>
        public int RequestDirectDebitMandate { get; set; }

        /// <summary>
        /// The end-user's date of birth.
        /// </summary>
        /// <example>1979-01-31</example>
        public string DateOfBirth { get; set; }

        /// <summary>
        /// Human-readable identifier of the consumer-facing merchant(e.g.legal name or trade name)
        /// </summary>
        /// <remarks>Mandatory attribute for Trustly Partners that are using Express Merchant Onboarding (EMO) and aggregate traffic under a master processing account.</remarks>
        /// <example>Merchant Ltd.</example>
        public string PSPMerchant { get; set; }

        /// <summary>
        /// URL of the consumer-facing website where the order is initiated
        /// </summary>
        /// <remarks>Mandatory attribute for Trustly Partners that are using Express Merchant Onboarding (EMO) and aggregate traffic under a master processing account.</remarks>
        /// <example>www.merchant.com</example>
        public string PSPMerchantURL { get; set; }

        /// <summary>
        /// VISA category codes describing the merchant's nature of business.
        /// <remarks>Mandatory attribute for Trustly Partners that are using Express Merchant Onboarding (EMO) and aggregate traffic under a master processing account.</remarks>
        /// </summary>
        public string MerchantCategoryCode { get; set; }

    }
}
