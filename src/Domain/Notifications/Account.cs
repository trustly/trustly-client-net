using Newtonsoft.Json;
using Trustly.Api.Domain.Base;

namespace Trustly.Api.Domain.Notifications
{
    public class AccountNotificationData : AbstractRequestParamsData<AccountNotificationDataAttributes>
    {
        [JsonProperty("messageid")]
        public string MessageID { get; set; }

        [JsonProperty("orderid")]
        public string OrderID { get; set; }

        [JsonProperty("notificationid")]
        public string NotificationID { get; set; }

        [JsonProperty("accountid")]
        public string AccountID { get; set; }

        [JsonProperty("verified")]
        public int Verified { get; set; }
    }

    public class AccountNotificationDataAttributes : AbstractRequestParamsDataAttributes
    {
        /// <summary>
        /// The clearinghouse for this account
        /// </summary>
        [JsonProperty("clearinghouse")]
        public string Clearinghouse { get; set; }

        /// <summary>
        /// The bank for this account
        /// </summary>
        [JsonProperty("bank")]
        public string Bank { get; set; }

        /// <summary>
        /// A text that is safe to show the enduser for identifying the account.Do not parse this text since it will be a different format for different accounts.
        /// </summary>
        [JsonProperty("descriptor")]
        public string Descriptor { get; set; }

        /// <summary>
        /// The last digits of the bank account number.This can be used for matching against received KYC data from your manual routines.
        /// </summary>
        [JsonProperty("lastdigits")]
        public string Lastdigits { get; set; }

        /// <summary>
        /// An ID that uniquely identifies the account holder.Note: The format of this field will for some countries look different than the example.
        /// </summary>
        [JsonProperty("personid")]
        public string PersonID { get; set; }

        /// <summary>
        /// The name of the account holder
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The address of the account holder
        /// </summary>
        [JsonProperty("address")]
        public string Address { get; set; }

        /// <summary>
        /// The zipcode of the account holder
        /// </summary>
        [JsonProperty("zipcode")]
        public string Zipcode { get; set; }

        /// <summary>
        /// The city of the account holder
        /// </summary>
        [JsonProperty("city")]
        public string City { get; set; }

        /// <summary>
        /// 1 if a direct debit mandate exists for this account, 0 otherwise
        /// </summary>
        [JsonProperty("directdebitmandate")]
        public int DirectDebitMandate { get; set; }
    }
}
