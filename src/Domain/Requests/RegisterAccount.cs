using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Trustly.Api.Domain.Base;
using Trustly.Api.Domain.Common;

namespace Trustly.Api.Domain.Requests
{
    public class RegisterAccountRequestData : AbstractToTrustlyRequestParamsData<RegisterAccountRequestDataAttributes>
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
    }

    public class RegisterAccountRequestDataAttributes : AbstractRequestParamsDataAttributes
    {
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

    public class RegisterAccountResponseData : AbstractResponseResultData
    {
        /// <summary>
        /// The globally unique AccountID the account was assigned in our system.
        /// </summary>
        /// <example>7653385737</example>
        [JsonProperty("accountid")]
        public string AccountId { get; set; }

        /// <summary>
        /// The clearinghouse for this account.
        /// </summary>
        /// <example>SWEDEN</example>
        [JsonProperty("clearinghouse")]
        public string ClearingHouse { get; set; }

        /// <summary>
        /// The name of the bank for this account.
        /// </summary>
        /// <example>Skandiabanken</example>
        [JsonProperty("bank")]
        public string Bank { get; set; }

        /// <summary>
        /// A descriptor for this account that is safe to show to the end user.
        /// </summary>
        /// <example>***4057</example>
        [JsonProperty("descriptor")]
        public string Descriptor { get; set; }
    }

    public class RegisterAccount
    {
        public RegisterAccount()
        {
        }
    }
}

