using Newtonsoft.Json;
using System;
using Trustly.Api.Domain.Base;

namespace Trustly.Api.Domain.Notifications
{
    public class KycNotificationData : AbstractFromTrustlyRequestParamsData<KycNotificationDataAttributes>
    {
        [JsonProperty("orderid")]
        public string OrderID { get; set; }

        [JsonProperty("messageid")]
        public string MessageID { get; set; }

        [JsonProperty("kycentityid")]
        public string KycEntityID { get; set; }

        [JsonProperty("notificationid")]
        public string NotificationID { get; set; }
    }

    public class KycNotificationDataAttributes: AbstractRequestParamsDataAttributes
    {
        /// <summary>
        /// Entity’s personal number (SSN)
        /// Can be considered as a unique identifier
        /// Only present in markets where SSN is applicable
        /// </summary>
        [JsonProperty("personid")]
        public string MessageID { get; set; }

        /// <summary>
        /// Entity’s first name.	
        /// </summary>
        [JsonProperty("firstname")]
        public string FirstName { get; set; }

        /// <summary>
        /// Entity’s last name.	
        /// </summary>
        [JsonProperty("lastname")]
        public string LastName { get; set; }

        /// <summary>
        /// Entity’s date of birth in YYYY-MM-DD format.	
        /// </summary>
        [JsonProperty("dob")]
        public DateTime DateOfBirth { get; set; }

        /// <summary>
        /// Entity’s street name.	
        /// </summary>
        [JsonProperty("street")]
        public string Street { get; set; }

        /// <summary>
        /// Entity’s ZIP code.	
        /// </summary>
        [JsonProperty("zipcode")]
        public string ZipCode { get; set; }

        /// <summary>
        /// Entity’s city.	
        /// </summary>
        [JsonProperty("city")]
        public string City { get; set; }

        /// <summary>
        /// Entity’s country of residence	
        /// </summary>
        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("abort")]
        public int? Abort { get; set; }

        [JsonProperty("abortmessage")]
        public string AbortMessage { get; set; }
    }
}
