﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Trustly.Api.Domain.Base;

namespace Trustly.Api.Domain.Requests
{
    public class GetWithdrawalsRequestData : AbstractToTrustlyRequestParamsData<EmptyRequestParamsDataAttributes>
    {
        public string OrderID { get; set; }
    }

    [JsonConverter(typeof(GetWithdrawalsResponseDataConverter))]
    public class GetWithdrawalsResponseData : AbstractResponseResultData
    {
        public List<GetWithdrawalsResponseDataEntry> Entries { get; set; }
    }

    public class GetWithdrawalsResponseDataConverter : JsonConverter<GetWithdrawalsResponseData>
    {
        public override GetWithdrawalsResponseData ReadJson(JsonReader reader, Type objectType, GetWithdrawalsResponseData existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jarray = JToken.Load(reader);
            return new GetWithdrawalsResponseData
            {
                Entries = jarray.ToObject<List<GetWithdrawalsResponseDataEntry>>()
            };
        }

        public override void WriteJson(JsonWriter writer, GetWithdrawalsResponseData value, JsonSerializer serializer)
        {
            JToken.FromObject(value.Entries).WriteTo(writer);
        }
    }

    public class GetWithdrawalsResponseDataEntry
    {
        /// <summary>
        /// Reference code for the withdrawal generated by Trustly.
        /// </summary>
        [JsonProperty("reference")]
        public string Reference { get; set; }

        /// <summary>
        /// Date and time when the withdrawal was last updated.
        /// </summary>
        [JsonProperty("modificationdate")]
        public string ModificationDate { get; set; }

        /// <summary>
        /// OrderID of the withdrawal
        /// </summary>
        [JsonProperty("orderid")]
        public string OrderID { get; set; }

        /// <summary>
        /// Date and time when the withdrawal request was received.
        /// </summary>
        [JsonProperty("datestamp")]
        public string Datestamp { get; set; }

        /// <summary>
        /// The current state* of the withdrawal.
        /// </summary>
        [JsonProperty("transferstate")]
        public string TransferState { get; set; }

        /// <summary>
        /// The amount of the withdrawal.
        /// </summary>
        [JsonProperty("amount")]
        public string Amount { get; set; }

        /// <summary>
        /// The accountid of the receiving account.
        /// </summary>
        [JsonProperty("accountid")]
        public string Accountid { get; set; }

        /// <summary>
        /// The currency of the withdrawal.
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }

        /// <summary>
        /// The estimated date and time for when the funds will be available on the receiving bank account.If this information is not available it will be null.
        /// </summary>
        [JsonProperty("eta")]
        public string Eta { get; set; }
    }
}
