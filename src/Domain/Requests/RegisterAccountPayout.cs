using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Trustly.Api.Domain.Base;
using Trustly.Api.Domain.Common;

namespace Trustly.Api.Domain.Requests
{
	public class RegisterAccountPayoutRequestData : AbstractToTrustlyRequestParamsData<RegisterAccountPayoutRequestDataAttributes>
	{
        /// <summary>
        /// ID, username, hash or anything uniquely identifying the end-user to be identified.
        /// Preferably the same ID/username as used in the merchant's own backoffice in order to simplify for the merchant's support department
        /// </summary>
        public string EndUserID { get; set; }

        /// <summary>
        /// The clearing house of the end-user's bank account. Typically the name of a country in uppercase letters. See table
        /// at https://developers.trustly.com/emea/docs/registeraccount 
        /// </summary>
        /// <example>SWEDEN</example>
        public string ClearingHouse { get; set; }

        /// <summary>
        /// The bank number identifying the end-user's bank in the given clearing house. For bank accounts in IBAN format you should just
        /// provide an empty string (""). For non-IBAN format, see table at https://developers.trustly.com/emea/docs/registeraccount 
        /// </summary>
        public string BankNumber { get; set; }

        /// <summary>
        /// The account number, identifying the end-user's account in the bank. Can be either IBAN or country-specific format, see table
        /// at https://developers.trustly.com/emea/docs/registeraccount
        /// </summary>
        public string AccountNumber { get; set; }

        /// <summary>
        /// First name of the account holder (or the name of the company/organization)
        /// </summary>
        public string Firstname { get; set; }

        /// <summary>
        /// Last name of the account holder (empty for organizations/companies)
        /// </summary>
        public string Lastname { get; set; }

        /// <summary>
        /// The URL to which notifications for this payment should be sent to. This URL should be hard to guess and not contain a ? ("question mark").
        /// </summary>
        [Required]
        public string NotificationURL { get; set; }

        /// <summary>
        /// Your unique ID for the payout.
        /// If the MessageID is a previously initiated P2P order then the payout will be attached to that P2P order and the amount must be equal to or lower than the previously deposited amount.
        /// </summary>
        [Required]
        public string MessageID { get; set; }

        /// <summary>
        /// The amount to send with exactly two decimals. Only digits. Use dot (.) as decimal separator.
        /// If the end-user holds a balance in the merchant's system then the amount must have been deducted from that balance before calling this method.
        /// </summary>
        [Required]
        public string Amount { get; set; }

        /// <summary>
        /// The currency of the end-user's account in the merchant's system.
        /// </summary>
        [Required]
        public string Currency { get; set; }
    }

	public class RegisterAccountPayoutRequestDataAttributes : AbstractRequestParamsDataAttributes
	{
        /// <summary>
        /// The text to show on the end-user's bank statement after Trustly's own 10 digit reference (which always will be displayed first).
        /// The reference must let the end user identify the merchant based on this value. So the ShopperStatement should contain either your brand name, website name, or company name.
        /// 
        /// If possible, try to keep this text as short as possible to maximise the chance that the full reference
        /// will fit into the reference field on the customer's bank since some banks allow only a limited number of characters.
        /// </summary>
        [Required]
        public string ShopperStatement { get; set; }

        /// <summary>
        /// The ExternalReference is a reference set by the merchant for any purpose and does not need to be unique
        /// for every API call. The ExternalReference will be included in version 1.2 of the settlement report, ViewAutomaticSettlementDetailsCSV.
        /// </summary>
        public string ExternalReference { get; set; }

        /// <summary>
        /// Human-readable identifier of the consumer-facing merchant (e.g. legal name or trade name)
        /// </summary>
        /// <remarks>Mandatory attribute for Trustly Partners that are using Express Merchant Onboarding (EMO) and aggregate traffic under a master processing account. It is also mandatory for E-wallets used directly in a merchant's checkout.</remarks>
        public string PSPMerchant { get; set; }

        /// <summary>
        /// URL of the consumer-facing website where the order is initiated
        //// </summary>
        // <remarks>Mandatory attribute for Trustly Partners that are using Express Merchant Onboarding (EMO) and aggregate traffic under a master processing account. It is also mandatory for E-wallets used directly in a merchant's checkout.</remarks>
        public string PSPMerchantURL { get; set; }

        /// <summary>
        /// VISA category codes describing the merchant's nature of business.
        /// </summary>
        /// <remarks>Mandatory attribute for Trustly Partners that are using Express Merchant Onboarding (EMO) and aggregate traffic under a master processing account. It is also mandatory for E-wallets used directly in a merchant's checkout.</remarks>
        public string MerchantCategoryCode { get; set; }

        /// <summary>
        /// Information about the Payer (ultimate debtor). This is required for some merchants and partners, see below. 
        /// </summary>
        public RecipientOrSenderInformation SenderInformation { get; set; }

        /// <summary>
        /// The end-user's date of birth.
        /// </summary>
        /// <example>1979-01-31</example>
        public string DateOfBirth { get; set; }

        /// <summary>
        /// The mobile phonenumber to the account holder in international format. This is used for KYC and AML routines.
        /// </summary>
        /// <example>+46709876543</example>
        public string MobilePhone { get; set; }

        /// <summary>
        /// The account holder's social security number / personal number / birth number / etc. Useful for some banks for identifying transactions and KYC/AML.
        /// </summary>
        /// <example>790131-1234</example>
        public string NationalIdentificationNumber { get; set; }

        /// <summary>
        /// The ISO 3166-1-alpha-2 code of the account holder's country.
        /// </summary>
        /// <example>SE</example>
        public string AddressCountry { get; set; }

        /// <summary>
        /// Postal code of the account holder.
        /// </summary>
        /// <example>SE-11253</example>
        public string AddressPostalCode { get; set; }

        /// <summary>
        /// City of the account holder.
        /// </summary>
        /// <example>Stockholm</example>
        public string AddressCity { get; set; }

        /// <summary>
        /// Street address of the account holder.
        /// </summary>
        /// <example>Main street 1</example>
        public string AddressLine1 { get; set; }

        /// <summary>
        /// Additional address information of the account holder.
        /// </summary>
        /// <example>Apartment 123, 2 stairs up</example>
        public string AddressLine2 { get; set; }

        /// <summary>
        /// The entire address of the account holder. This attribute should only be used if you are unable to provide
        /// the address information in the 5 separate attributes above (AddressCountry, AddressPostalCode, AddressCity, AddressLine1 and AddressLine2).
        /// </summary>
        /// <example>Birgerstreet 14, SE-11411 Stockholm, Sweden</example>
        public string Address { get; set; }

        /// <summary>
        /// The email address of the account holder.
        /// </summary>
        /// <example>test@trustly.com</example>
        public string Email { get; set; }
    }

	public class RegisterAccountPayoutResponseData : AbstractResponseResultData
	{
        /// <summary>
        /// The globally unique OrderID the account payout order was assigned in our system.
        /// </summary>
        [JsonProperty("orderid")]
        public long OrderID { get; set; }

        /// <summary>
        /// "1" if the payout could be accepted and "0" otherwise.
        /// </summary>
        [JsonProperty("result")]
        [JsonConverter(typeof(StringBooleanJsonConverter))]
        public bool Result { get; set; }

    }

	public class RegisterAccountPayout
	{
		public RegisterAccountPayout()
		{
		}
	}
}

