using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Trustly.Api.Domain.Requests;

namespace Trustly.Api.Client
{
    public class SettlementReportParser
    {
        private delegate void Mapper(SettlementReportResponseDataRow row, string value);

        private readonly static Mapper NOOP_MAPPER = (row, value) => { };
        private readonly static Regex EXP_CSV_VALUE = new Regex("(?:^|,)(?=[^\"]| (\")?)\" ? ((? (1)[^\"]*|[^,\"] *))\"?(?=,|$)", RegexOptions.Compiled | RegexOptions.Singleline);

        private readonly Dictionary<string, Mapper> _mappers = new();

        public SettlementReportParser()
        {
            this._mappers.Add("accountname", (row, value) => row.AccountName = value);
            this._mappers.Add("currency", (row, value) => row.Currency = value);
            this._mappers.Add("messageid", (row, value) => row.MessageID = value);
            this._mappers.Add("orderid", (row, value) => row.OrderID = value);
            this._mappers.Add("ordertype", (row, value) => row.OrderType = value);
            this._mappers.Add("username", (row, value) => row.Username = value);
            this._mappers.Add("fxpaymentcurrency", (row, value) => row.FxPaymentCurrency = value);
            this._mappers.Add("settlementbankwithdrawalid", (row, value) => row.SettlementBankWithdrawalID = value);
            this._mappers.Add("externalreference", (row, value) => row.ExternalReference = value);
            this._mappers.Add("extraref", (row, value) => row.ExternalReference = value);

            this._mappers.Add("amount", (row, value) =>
            {
                if (!decimal.TryParse(value, out decimal result))
                {
                    throw new ArgumentException($"Could not convert value '${value}' into a decimal");
                }

                row.Amount = result;
            });

            this._mappers.Add("fxpaymentamount", (row, value) =>
            {
                if (!decimal.TryParse(value, out decimal result))
                {
                    throw new ArgumentException($"Could not convert value '${value}' into a decimal");
                }

                row.FxPaymentAmount = result;
            });

            this._mappers.Add("total", (row, value) =>
            {
                if (!decimal.TryParse(value, out decimal result))
                {
                    throw new ArgumentException($"Could not convert value '${value}' into a decimal");
                }

                row.Total = result;
            });

            this._mappers.Add("datestamp", (row, value) =>
            {
                if (!DateTime.TryParse(value, out DateTime result))
                {
                    throw new ArgumentException($"Could not convert value '${value}' into a decimal");
                }

                row.Datestamp = result;
            });
        }

        public List<SettlementReportResponseDataRow> Parse(string csv)
        {

            var lines = csv.Replace("\r", "").Split('\n');
            var rows = new List<SettlementReportResponseDataRow>();

            if (lines.Length == 0)
            {
                return rows;
            }

            var headers = lines[0].Split(',');

            var localMappers = new List<Mapper>();
            foreach (var header in headers)
            {
                var lowerCaseHeaderKey = header.ToLower(CultureInfo.InvariantCulture);
                if (this._mappers.ContainsKey(lowerCaseHeaderKey))
                {
                    localMappers.Add(this._mappers?[lowerCaseHeaderKey]);
                }
                else
                {
                    // We do not recognize this new header key.
                    // This could count as an error, but we will let it go.
                    // The preferred way would perhaps be to log about the lost data,
                    // but we do not want to include a dependency on a logging library.
                    localMappers.Add(NOOP_MAPPER);
                }
            }

            for (var i = 1; i < lines.Length; i++)
            {
                var line = lines[i];

                var parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(new StringReader(line));
                parser.HasFieldsEnclosedInQuotes = true;
                parser.SetDelimiters(",");

                while (!parser.EndOfData)
                {
                    var fields = parser.ReadFields();

                    var row = new SettlementReportResponseDataRow();
                    for (var columnIndex = 0; columnIndex < fields.Length; columnIndex++)
                    {
                        if (string.IsNullOrEmpty(fields[columnIndex]) == false)
                        {
                            localMappers[columnIndex](row, fields[columnIndex]);
                        }
                    }

                    rows.Add(row);
                }
            }

            return rows;
        }
    }
}
