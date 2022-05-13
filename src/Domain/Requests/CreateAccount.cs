using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Trustly.Api.Domain.Base;
using Trustly.Api.Domain.Common;

namespace Trustly.Api.Domain.Requests
{
    public class CreateAccountRequestData : AbstractToTrustlyRequestParamsData<CreateAccountRequestDataAttributes>
    {
        /// <summary>
        /// ID, username, hash or anything uniquely identifying the end-user holding this account.
        ///
        /// Preferably the same ID/username as used in the merchant's own backoffice in order to simplify for the merchant's support department.
        /// </summary>
        [Required]
        public string EndUserID { get; set; }

        /// <summary>
        /// The clearing house of the end-user's bank account. Typically the name of a country in uppercase letters. See table* below.
        /// </summary>
        [Required]
        public string ClearingHouse { get; set; }

        /// <summary>
        /// The bank number identifying the end-user's bank in the given clearing house. For bank accounts in IBAN format you should just provide an empty string (""). For non-IBAN format, see table* below.
        /// </summary>
        [Required]
        public string BankNumber { get; set; }

        /// <summary>
        /// The account number, identifying the end-user's account in the bank. Can be either IBAN or country-specific format, see table* below.
        /// </summary>
        [Required]
        public string AccountNumber { get; set; }
        /// <summary>
        /// First name of the account holder (or the name of the company/organization)
        /// </summary>
        [Required]
        public string Firstname { get; set; }

        /// <summary>
        /// Last name of the account holder(empty for organizations/companies)
        /// </summary>
        [Required]
        public string Lastname { get; set; }
    }

    public class CreateAccountResponseData : AbstractResponseResultData
    {
        /// <summary>
        /// The globally unique AccountID the account was assigned in our system.
        /// </summary>
        [JsonProperty("accountid")]
        public string AccountID { get; set; }

        /// <summary>
        /// The clearinghouse for this account.
        /// </summary>
        [JsonProperty("clearinghouse")]
        public string ClearingHouse { get; set; }

        /// <summary>
        /// The name of the bank for this account.
        /// </summary>
        [JsonProperty("bank")]
        public string Bank { get; set; }

        /// <summary>
        /// A descriptor for this account that is safe to show to the end user.
        /// </summary>
        [JsonProperty("descriptor")]
        public string Descriptor { get; set; }
    }

    public class CreateAccountRequestDataAttributes : AbstractRequestParamsDataAttributes
    {
        /// <summary>
        /// The date of birth of the account holder(ISO 8601).
        /// </summary>
        public string DateOfBirth { get; set; }

        /// <summary>
        /// The mobile phonenumber to the account holder in international format.This is used for KYC and AML routines.
        /// </summary>
        public string MobilePhone { get; set; }

        /// <summary>
        /// The account holder's social security number / personal number / birth number / etc.
        /// Useful for some banks for identifying transactions and KYC/AML.
        /// </summary>
        public string NationalIdentificationNumber { get; set; }

        /// <summary>
        /// The ISO 3166-1-alpha-2 code of the account holder's country.
        /// </summary>
        public string AddressCountry { get; set; }

        /// <summary>
        /// Postal code of the account holder.
        /// </summary>
        public string AddressPostalCode { get; set; }

        /// <summary>
        /// City of the account holder.
        /// </summary>
        public string AddressCity { get; set; }

        /// <summary>
        /// Street address of the account holder.
        /// </summary>
        public string AddressLine1 { get; set; }

        /// <summary>
        /// Additional address information of the account holder.
        /// </summary>
        public string AddressLine2 { get; set; }

        /// <summary>
        /// The entire address of the account holder.
        /// This attribute should only be used if you are unable to provide the address information in the 5 separate attributes:
        /// 
        /// <ul>
        ///   <li>AddressCountry</li>
        ///   <li>AddressPostalCode</li>
        ///   <li>AddressCity</li>
        ///   <li>AddressLine1</li>
        ///   <li>AddressLine2</li>
        /// </ul>
        /// 
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// The email address of the account holder.
        /// </summary>
        public string Email { get; set; }
    }
}
