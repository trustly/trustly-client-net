using System;
using Newtonsoft.Json;
using Trustly.Api.Domain.Base;

namespace Trustly.Api.Domain.Requests
{
    public class GetAccountTransactionsRequestData : AbstractToTrustlyRequestParamsData<GetAccountTransactionsRequestAttributes>
    {
        /// <summary>
        /// The Action static value GetConsentURL.
        /// </summary>
        public string Action = "GetConsentURL";
        
        /// <summary>
        /// The notification URL to send callbacks.
        /// </summary>
        public string NotificationURL { get; set; }

        /// <summary>
        /// The unique identifier for the end user.
        /// </summary>
        public string EndUserID { get; set; }

        /// <summary>
        /// The unique message identifier for the request.
        /// </summary>
        public string MessageID { get; set; }
        
        /// <summary>
        /// The ISO 3166-1-alpha-2 country code.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// The locale in the format "language_REGION" (e.g., "sv_SE").
        /// </summary>
        public string Locale { get; set; }

        /// <summary>
        /// The URL to redirect the user on success.
        /// </summary>
        public string SuccessURL { get; set; }

        /// <summary>
        /// The URL to redirect the user on failure.
        /// </summary>
        public string FailURL { get; set; }
    }

    public class GetAccountTransactionsResponseData : AbstractResponseResultData
    {
        /// <summary>
        /// The OrderID specified when calling the method.
        /// </summary>
        [JsonProperty("orderid")]
        public string OrderID { get; set; }

        /// <summary>
        /// The URL that should be loaded so that the end-user can complete the deposit.
        /// </summary>
        [JsonProperty("url")]
        public string URL { get; set; }
    }
    
    /// <summary>
    /// Represents the attributes associated with the request.
    /// </summary>
    public class GetAccountTransactionsRequestAttributes : AbstractRequestParamsDataAttributes
    {
        /// <summary>
        /// The IP address of the end user.
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// The mobile phone number of the end user.
        /// </summary>
        public string MobilePhone { get; set; }

        /// <summary>
        /// The first name of the end user.
        /// </summary>
        public string Firstname { get; set; }

        /// <summary>
        /// The last name of the end user.
        /// </summary>
        public string Lastname { get; set; }

        /// <summary>
        /// The email address of the end user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The national identification number of the end user.
        /// </summary>
        public string NationalIdentificationNumber { get; set; }

        /// <summary>
        /// Indicates whether the date of birth is  "1979-01-31".
        /// </summary>
        public string DateOfBirth { get; set; }
    }
}