﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Trustly.Api.Domain.Base;

namespace Trustly.Api.Domain.Requests
{
    public class AccountLedgerRequestData : AbstractToTrustlyRequestParamsData<EmptyRequestParamsDataAttributes>
    {
        [Required]
        public string FromDate { get; set; }

        [Required]
        public string ToDate { get; set; }

        [Required]
        public string Currency { get; set; }
    }

    [JsonConverter(typeof(AccountLedgerResponseDataConverter))]
    public class AccountLedgerResponseData : AbstractResponseResultData
    {
        public List<AccountLedgerResponseDataEntry> Entries { get; set; }
    }

    public class AccountLedgerResponseDataConverter : JsonConverter<AccountLedgerResponseData>
    {
        public override AccountLedgerResponseData ReadJson(JsonReader reader, Type objectType, AccountLedgerResponseData existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jarray = JToken.Load(reader);
            return new AccountLedgerResponseData
            {
                Entries = jarray.ToObject<List<AccountLedgerResponseDataEntry>>()
            };
        }

        public override void WriteJson(JsonWriter writer, AccountLedgerResponseData value, JsonSerializer serializer)
        {
            JToken.FromObject(value.Entries).WriteTo(writer);
        }
    }

    public class AccountLedgerResponseDataEntry
    {
        /// <summary>
        /// Your userid in our system.
        /// </summary>
        [JsonProperty("userid")]
        public string UserID { get; set; }

        /// <summary>
        /// The datestamp for when this ledger row affected your balance in our system.
        /// </summary>
        [JsonProperty("datestamp")]
        public string Datestamp { get; set; }

        /// <summary>
        /// The globally unique OrderID that resulted in this ledger record.
        /// </summary>
        [JsonProperty("orderid")]
        public string OrderID { get; set; }

        /// <summary>
        /// The name of the bookkeeping account this ledger record belongs to.
        /// </summary>
        [JsonProperty("accountname")]
        public string AccountName { get; set; }

        /// <summary>
        /// Your unique MessageID that you used to create the order that resulted in this ledger record.
        /// </summary>
        [JsonProperty("messageid")]
        public string MessageID { get; set; }

        /// <summary>
        /// A human friendly description of this ledger record.
        /// </summary>
        [JsonProperty("transactiontype")]
        public string TransactionType { get; set; }

        /// <summary>
        /// The currency of the amount in this ledger record.
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }

        /// <summary>
        /// The amount your balance in our system was affected with due to this ledger record. May contain a lot of decimals.
        /// </summary>
        [JsonProperty("amount")]
        public string Amount { get; set; }

        /// <summary>
        /// An ID meaning different things for different payment methods, you probably don't need this data.
        /// </summary>
        [JsonProperty("gluepayid")]
        public string GluepayID { get; set; }
    }
}
