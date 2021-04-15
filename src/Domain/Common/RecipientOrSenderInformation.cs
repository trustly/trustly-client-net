using System;
using Newtonsoft.Json;

namespace Trustly.Api.Domain.Common
{
    public class RecipientOrSenderInformation
    {
        /// <summary>
        /// Partytype can be "PERSON" or "ORGANISATION" (if the recipient is an organisation/company).
        /// </summary>
        [JsonProperty("Partytype")]
        public string Partytype { get; set; }

        /// <summary>
        /// First name of the person, or the name of the organisation.
        /// </summary>
        [JsonProperty("Firstname")]
        public string Firstname { get; set; }

        /// <summary>
        /// Last name of the person (NULL for organisation).
        /// </summary>
        [JsonProperty("Lastname")]
        public string Lastname { get; set; }

        /// <summary>
        /// The ISO 3166-1-alpha-2 code of the country that the recipient resides in.
        /// </summary>
        [JsonProperty("CountryCode")]
        public string CountryCode { get; set; }

        /// <summary>
        /// Payment account number or an alternative consistent unique identifier(e.g.customer number).
        /// Note: this is not a transaction ID or similar.This identifier must stay consistent across all transactions relating to this recipient (payee).
        /// </summary>
        [JsonProperty("CustomerID")]
        public string CustomerID { get; set; }

        /// <summary>
        /// Full address of the recipient, excluding the country.
        /// </summary>
        [JsonProperty("Address")]
        public string Address { get; set; }

        /// <summary>
        /// Date of birth (YYYY-MM-DD) of the beneficiary, or organisational number for the organisation.
        /// </summary>
        [JsonProperty("DateOfBirth")]
        public string DateOfBirth { get; set; }
    }
}
