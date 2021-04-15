using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Trustly.Api.Domain.Base;

namespace Trustly.Api.Domain.Requests
{
    public class SettlementReportRequestData : AbstractRequestParamsData<SettlementReportRequestDataAttributes>
    {
        /// <summary>
        /// If the value is specified (i.e. not "null"), the system will only search for a settlement executed in that particular currency. If unspecified, settlements executed in any currency are included in the report.
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// The date when the settlement was processed.
        /// </summary>
        /// <example>2014-04-01</example>
        public string SettlementDate { get; set; }
    }

    public class SettlementReportRequestDataAttributes : AbstractRequestParamsDataAttributes
    {
        /// <summary>
        /// Required.
        /// The APIVersion. Must be "1.2". We also have older versions of the report, but those should not be implemented by new merchants.
        /// </summary>
        public string APIVersion { get; set; }
    }

    public class SettlementReportResponseData : AbstractResponseResultData
    {
        [JsonProperty("view_automatic_settlement_details")]
        public string CsvContent { get; set; }

        public List<SettlementReportResponseDataRow> Rows { get; set; }
    }

    public class SettlementReportResponseDataRow
    {
        /// <summary>
        /// The account the money was transferred from(if the amount is positive), or the account the money was transferred to(if the amount is negative).
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// The monetary amount of the transaction, rounded down to two decimal places.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The three-letter currency code of the transaction.
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// The timestamp of the transaction, including the UTC offset.As the timestamps are always in UTC, the offset is always +00
        /// </summary>
        /// <example>2014-03-31 11:50:06.46106+00</example>
        public DateTime Datestamp { get; set; }

        /// <summary>
        /// MessageID of the order associated with the transaction, if available.
        /// </summary>
        public string Messageid { get; set; }

        /// <summary>
        /// OrderID of the order associated with the transaction, if available.
        /// </summary>
        public string Orderid { get; set; }

        /// <summary>
        /// The type of the order associated with the transaction, if available.Text See list of possible orderypes in the table below.
        /// </summary>
        public string Ordertype { get; set; }

        /// <summary>
        /// The sum of all amounts of the respective currency within the report.
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// The username of the child merchant account.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The amount that the end user paid, if the currency is different from the requested deposit currency. For transactions where the payment currency is the same as the requested currency, this field will be empty.
        /// </summary>
        public decimal FxPaymentAmount { get; set; }

        /// <summary>
        /// The currency that the user paid with, if the currency is different from the requested deposit currency. For transactions where the payment currency is the same as the requested currency, this field will be empty.
        /// </summary>
        public string FxPaymentCurrency { get; set; }

        /// <summary>
        /// The 10 digit reference that will show up on the merchant's bank statement for this automatic settlement batch. The same value will be sent on every row in the report.
        /// </summary>
        public string SettlementBankWithdrawalID { get; set; }

        /// <summary>
        /// Contains the ExternalReference value for Deposit, Charge, and Refund transactions if provided.Otherwise empty.
        /// </summary>
        public string ExternalReference { get; set; }
    }
}