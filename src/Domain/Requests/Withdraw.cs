using System;
using Newtonsoft.Json;
using Trustly.Api.Domain.Base;
using Trustly.Api.Domain.Common;

namespace Trustly.Api.Domain.Requests
{
    public class WithdrawRequestData : AbstractToTrustlyRequestParamsData<WithdrawRequestDataAttributes>
    {
        /// <summary>
        /// The URL to which notifications for this payment should be sent to. This URL should be hard to guess and not contain a ? ("question mark").
        /// </summary>
        public string NotificationURL { get; set; }

        /// <summary>
        /// ID, username, hash or anything uniquely identifying the end-user requesting the withdrawal,
        /// Preferably the same ID/username as used in the merchant's own backoffice in order to simplify for the merchant's support department.
        /// </summary>
        public string EndUserID { get; set; }

        /// <summary>
        /// Your unique ID for the withdrawal.
        /// </summary>
        public string MessageID { get; set; }

        /// <summary>
        /// The currency of the end-user's account in the merchant's system.
        /// </summary>
        public string Currency { get; set; }
    }

    public class WithdrawResponseData : AbstractResponseResultData
    {
        /// <summary>
        /// The globally unique OrderID the withdrawal was assigned in our system.
        /// </summary>
        [JsonProperty("orderid")]
        public long OrderId { get; set; }

        /// <summary>
        /// The URL that should be loaded so that the end-user can complete the withdrawal.
        /// </summary>
        [JsonProperty("url")]
        public string URL { get; set; }
    }

    public class WithdrawRequestDataAttributes : AbstractDepositAndWithdrawDataAttributes
    {
        /// <summary>
        /// Sets a fixed withdrawal amount which cannot be changed by the end-user in the Trustly iframe.
        /// If this attribute is not sent, the end-user will be asked to select the withdrawal amount in the Trustly iframe
        /// 
        /// Do not use in combination with <see cref="AbstractDepositAndWithdrawDataAttributes.SuggestedMinAmount"/> and <see cref="AbstractDepositAndWithdrawDataAttributes.SuggestedMaxAmount"/>.
        /// 
        /// Use dot(.) as decimal separator.
        /// </summary>
        public string SuggestedAmount { get; set; }

        /// <summary>
        /// The end-user's first name.
        /// </summary>
        public string DateOfBirth { get; set; }

        /// <summary>
        /// The ISO 3166-1-alpha-2 code of the shipping address country.
        /// </summary>
        public string AddressCountry { get; set; }

        /// <summary>
        /// The postalcode of the shipping address.
        /// </summary>
        public string AddressPostalCode { get; set; }

        /// <summary>
        /// The city of the shipping address.
        /// </summary>
        public string AddressCity { get; set; }

        /// <summary>
        /// Shipping address street
        /// </summary>
        public string AddressLine1 { get; set; }

        /// <summary>
        /// Additional shipping address information.
        /// </summary>
        public string AddressLine2 { get; set; }

        /// <summary>
        /// The entire shipping address.
        /// This attribute should only be used if you are unable to provide the shipping address information in the 5 separate properties:
        /// * <see cref="AddressCountry"/>
        /// * <see cref="AddressCity"/>
        /// * <see cref="AddressPostalCode"/>
        /// * <see cref="AddressLine1"/>
        /// * <see cref="AddressLine2"/>
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Information about the Payer (ultimate debtor).
        ///
        /// Mandatory for certain types of merchants and partners.
        /// SenderInformation is mandatory to send in Attributes{} for money transfer services (including remittance houses),
        /// e-wallets, prepaid cards, as well as for Trustly Partners that are using Express Merchant Onboarding and aggregate traffic under a master processing account (other cases may also apply).
        /// </summary>
        public RecipientOrSenderInformation SenderInformation { get; set; }
    }
}
