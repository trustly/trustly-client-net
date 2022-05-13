using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Trustly.Api.Domain.Base;

namespace Trustly.Api.Domain.Common
{
    public abstract class AbstractDepositAndWithdrawAndSelectAccountDataAttributes : AbstractRequestParamsDataAttributes
    {
        /// <summary>
        /// The end-user's first name.
        /// </summary>
        [Required]
        public string Firstname { get; set; }

        /// <summary>
        /// The end-user's last name.
        /// </summary>
        [Required]
        public string Lastname { get; set; }

        /// <summary>
        /// The ISO 3166-1-alpha-2 code of the end-user's country. This will be used for pre-selecting the country for the end-user in the iframe. Note: This will only have an effect for new end-users.If an end-user has done a previous order(with the same EndUserID), the country that was last used will be pre-selected.
        /// </summary>
        [Required]
        public string Country { get; set; }

        /// <summary>
        /// The end-users localization preference in the format[language[_territory]]. Language is the ISO 639-1 code and territory the ISO 3166-1-alpha-2 code.
        /// </summary>
        [Required]
        public string Locale { get; set; }

        /// <summary>
        /// The text to show on the end-user's bank statement after Trustly's own 10 digit reference(which always will be displayed first). The reference must let the end user identify the merchant based on this value.So the ShopperStatement should contain either your brand name, website name, or company name.
        /// </summary>
        [Required(ErrorMessage = "The ShopperStatement is required")]
        public string ShopperStatement { get; set; }

        /// <summary>
        /// The email address of the end user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The IP-address of the end-user.
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// The URL to which the end-user should be redirected after a successful deposit. Do not put any logic on that page since it's not guaranteed that the end-user will in fact visit it.
        /// </summary>
        /// <example>https://example.com/thank_you.html</example>
        public string SuccessURL { get; set; }

        /// <summary>
        /// The URL to which the end-user should be redirected after a failed deposit. Do not put any logic on that page since it's not guaranteed that the end-user will in fact visit it.
        /// </summary>
        /// <example>https://trustly.com/error.html</example>
        public string FailURL { get; set; }

        /// <summary>
        /// The TemplateURL should be used if you want to design your own payment page but have it hosted on Trustly's side.
        /// The URL of your template page should be provided in this attribute in every Deposit API call.
        /// Our system will then fetch the content of your template page, insert the Trustly iframe into it and host the entire page on Trustly’s side.
        /// In the response to the Deposit request, you will receive a URL to the hosted template page which you should redirect the user to (the hosted page cannot be iframed).
        /// </summary>
        public string TemplateURL { get; set; }

        /// <summary>
        /// The html target/framename of the SuccessURL. Only _top, _self and _parent are suported.
        /// </summary>
        public string URLTarget { get; set; }

        /// <summary>
        /// The mobile phone number of the end-user in international format.
        /// </summary>
        public string MobilePhone { get; set; }

        /// <summary>
        /// The end-user's social security number / personal number / birth number / etc. Useful for some banks for identifying transactions and KYC/AML. If a Swedish personid ("personnummer") is provided, it will be pre-filled when the user logs in to their bank.
        /// </summary>
        public string NationalIdentificationNumber { get; set; }

        /// <summary>
        /// This attribute disables the possibility to change/type in national identification number when logging in to a Swedish bank.If this attribute is sent, the attribute NationalIdentificationNumber needs to be correctly included in the request. Note: This is only available for Swedish banks.
        /// </summary>
        public string UnchangeableNationalIdentificationNumber { get; set; }

        /// <summary>
        /// If you are using Trustly from within your native iOS app, this attribute should be sent so that we can redirect the users back to your app in case an external app is used for authentication (for example Mobile Bank ID in Sweden).
        /// </summary>
        public string URLScheme { get; set; }
    }

    public abstract class AbstractDepositAndWithdrawDataAttributes : AbstractDepositAndWithdrawAndSelectAccountDataAttributes
    {
        /// <summary>
        /// The minimum amount the end-user is allowed to deposit in the currency specified by Currency.Only digits. Use dot (.) as decimal separator.
        /// </summary>
        public string SuggestedMinAmount { get; set; }

        /// <summary>
        /// The maximum amount the end-user is allowed to deposit in the currency specified by Currency.Only digits. Use dot (.) as decimal separator.
        /// </summary>
        public string SuggestedMaxAmount { get; set; }
    }
}
