using System;
using Newtonsoft.Json;
using Trustly.Api.Domain.Base;
using Trustly.Api.Domain.Common;

namespace Trustly.Api.Domain.Requests
{
    public class DepositRequestData : AbstractToTrustlyRequestParamsData<DepositRequestDataAttributes>
    {
        public string NotificationURL { get; set; }

        public string EndUserID { get; set; }

        public string MessageID { get; set; }
    }

    public class DepositResponseData : AbstractResponseResultData
    {
        /// <summary>
        /// The OrderID specified when calling the method.
        /// </summary>
        [JsonProperty("orderid")]
        public string OrderID { get; set; }

        /// <summary>
        /// 1 if the withdrawal could be approved and 0 otherwise.
        /// </summary>
        [JsonProperty("result")]
        public bool Result { get; set; }
    }

    public class DepositRequestDataAttributes : AbstractDepositAndWithdrawDataAttributes
    {
        /// <summary>
        /// iDeal.
        ///
        /// The iDEAL integration offered by Trustly allows for both iDEAL and Trustly payments on a single integration with all transactions visible in the same AccountLedger.
        /// To initiate a new iDEAL payment, add Method = "deposit.bank.netherlands.ideal" to the Deposit attributes.
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// The currency of the end-user's account in the merchant's system.
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// The amount to deposit with exactly two decimals in the currency specified by Currency.
        /// Do not use this attribute in combination with <see cref="AbstractDepositAndWithdrawDataAttributes.SuggestedMinAmount"/> and <see cref="AbstractDepositAndWithdrawDataAttributes.SuggestedMaxAmount"/>..
        /// Only digits. Use dot (.) as decimal separator.
        /// </summary>
        public string Amount { get; set; }

        /// <summary>
        /// The ISO 3166-1-alpha-2 code of the shipping address country.
        /// </summary>
        public string ShippingAddressCountry { get; set; }

        /// <summary>
        /// The postalcode of the shipping address.
        /// </summary>
        public string ShippingAddressPostalCode { get; set; }

        /// <summary>
        /// The city of the shipping address.
        /// </summary>
        public string ShippingAddressCity { get; set; }

        /// <summary>
        /// Shipping address street
        /// </summary>
        public string ShippingAddressLine1 { get; set; }

        /// <summary>
        /// Additional shipping address information.
        /// </summary>
        public string ShippingAddressLine2 { get; set; }

        /// <summary>
        /// The entire shipping address.
        /// This attribute should only be used if you are unable to provide the shipping address information in the 5 separate attributes above
        /// (ShippingAddressCountry, ShippingAddressCity, ShippingAddressPostalCode, ShippingAddressLine1, ShippingAddressLine2)
        /// </summary>
        public string ShippingAddress { get; set; }


        /// <summary>
        /// In addition to the deposit, request a direct debit mandate from the account used for the deposit. 1 enables, 0 disables.
        /// The default is disabled. If this attribute is set, additional account notifications might be sent.
        /// You can read more about Trustly Direct Debit here, under section 2.1
        /// </summary>
        public string RequestDirectDebitMandate { get; set; }

        /// <summary>
        /// The AccountID received from an account notification which shall be charged in a Trustly Direct Debit deposit.
        /// This attribute should only be sent in combination with "QuickDeposit" : 1
        /// </summary>
        public string ChargeAccountID { get; set; }

        /// <summary>
        /// Set to 1 for Trustly Direct Debit deposits. QuickDeposit should be set set to 1 when the end user attempts a quick deposit,
        /// even if ChargeAccountID is not set. You can read more about QuickDeposits here, under section 1.1 and 1.2.
        /// </summary>
        public int QuickDeposit { get; set; }

        /// <summary>
        /// The ExternalReference is a reference set by the merchant for any purpose and does not need to be unique for every API call.
        /// The ExternalReference will be included in version 1.2 of the settlement report, ViewAutomaticSettlementDetailsCSV.
        /// </summary>
        public string ExternalReference { get; set; }

        /// <summary>
        /// Human-readable identifier of the consumer-facing merchant (e.g. legal name or trade name)
        /// </summary>
        public string PSPMerchant { get; set; }

        /// <summary>
        /// URL of the consumer-facing website where the order is initiated
        /// </summary>
        public string PSPMerchantURL { get; set; }

        /// <summary>
        /// VISA category codes describing the merchant's nature of business.
        /// </summary>
        public string MerchantCategoryCode { get; set; }

        /// <summary>
        /// Information about the Payee (ultimate creditor).
        /// The burden of identifying who the Payee for any given transaction is lies with the Trustly customer.
        ///
        /// Required for some merchants and partners.
        ///
        /// RecipientInformation{} is mandatory to send for money transfer services (including remittance houses), e-wallets, prepaid cards,
        /// as well as for Trustly Partners that are using Express Merchant Onboarding and aggregate traffic under a master processing account (other cases may also apply).
        /// </summary>
        public RecipientOrSenderInformation RecipientInformation { get; set; }
    }
}
