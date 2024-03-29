﻿using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Trustly.Api.Domain.Base
{
    public interface IToTrustlyRequestParamsData : IRequestParamsData
    {
        string Username { get; set; }
        string Password { get; set; }
    }

    public class AbstractToTrustlyRequestParamsData<TAttr> : AbstractRequestParamsData<TAttr>, IToTrustlyRequestParamsData
        where TAttr : AbstractRequestParamsDataAttributes
    {
        /// <summary>
        /// You do not have to set this property.
        /// It is set automatically by the API Client.
        /// </summary>
        [Required]
        [JsonProperty("Username")]
        public string Username { get; set; }

        /// <summary>
        /// You do not have to set this property.
        /// It is set automatically by the API Client.
        /// </summary>
        [Required]
        [JsonProperty("Password")]
        public string Password { get; set; }

        [JsonProperty(PropertyName = "Attributes", NullValueHandling = NullValueHandling.Ignore)]
        public TAttr Attributes { get; set; }
    }
}
