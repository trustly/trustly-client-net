using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Trustly.Api.Domain.Base;

namespace Trustly.Api.Domain.Requests
{
    public class BalanceRequestData : AbstractToTrustlyRequestParamsData<EmptyRequestParamsDataAttributes>
    {
    }

    [JsonConverter(typeof(BalanceResponseDataConverter))]
    public class BalanceResponseData : AbstractResponseResultData
    {
        public List<BalanceResponseDataEntry> Entries { get; set; }
    }

    public class BalanceResponseDataConverter : JsonConverter<BalanceResponseData>
    {
        public override BalanceResponseData ReadJson(JsonReader reader, Type objectType, BalanceResponseData existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jarray = JToken.Load(reader);
            return new BalanceResponseData
            {
                Entries = jarray.ToObject<List<BalanceResponseDataEntry>>()
            };
        }

        public override void WriteJson(JsonWriter writer, BalanceResponseData value, JsonSerializer serializer)
        {
            JToken.FromObject(value.Entries).WriteTo(writer);
        }
    }

    public class BalanceResponseDataEntry
    {
        /// <summary>
        /// 	The currency
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }

        /// <summary>
        /// The balance with 2 decimals
        /// </summary>
        [JsonProperty("balance")]
        public string Balance { get; set; }
    }
}
