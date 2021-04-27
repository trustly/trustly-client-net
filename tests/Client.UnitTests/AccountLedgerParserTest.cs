using System;
using Newtonsoft.Json;
using NUnit.Framework;
using Trustly.Api.Domain.Base;
using Trustly.Api.Domain.Requests;

namespace Trustly.Api.Client.UnitTests
{
    public class AccountLedgerParserTest
    {
        [Test]
        public void TestParser()
        {
            var jsonResponse = @"
            {
                ""version"": ""1.1"",
                ""result"": {
                    ""signature"": ""lCteM8iCg++7uyF...TYoY/mc7eUQ6FWNPg=="",
                    ""method"": ""AccountLedger"",
                    ""data"": [
                        {
                            ""userid"": ""3839426635"",
                            ""datestamp"": ""2014-01-30 13:28:45.652299+01"",
                            ""orderid"": ""3209647863"",
                            ""accountname"": ""SUSPENSE_ACCOUNT_CLIENT_FUNDS_SWEDEN_ESSE"",
                            ""messageid"": ""133921"",
                            ""transactiontype"": ""User deposit of client funds to CLIENT_FUNDS_SWEDEN_ESSE"",
                            ""currency"": ""EUR"",
                            ""amount"": ""5.00000000000000000000"",
                            ""gluepayid"": ""3209647863""
                        },
                        {
                            ""accountname"": ""TRANSACTION_FEE_BANK_DEPOSIT"",
                            ""orderid"": ""3209647863"",
                            ""userid"": ""3839426635"",
                            ""datestamp"": ""2014-01-30 13:28:45.652299+01"",
                            ""messageid"": ""133921"",
                            ""transactiontype"": ""User deposit of client funds to CLIENT_FUNDS_SWEDEN_ESSE"",
                            ""currency"": ""SEK"",
                            ""amount"": ""-3.01"",
                            ""gluepayid"": ""3209647863""
                        }
                    ],
                    ""uuid"": ""9e4345db-6093-bb35-07d3-e335f1e28793""
                }
            }";

            var rpcResponse = JsonConvert.DeserializeObject<JsonRpcResponse<AccountLedgerResponseData>>(jsonResponse);

            Assert.AreEqual(2, rpcResponse.Result.Data.Entries.Count);

            Assert.AreEqual("9e4345db-6093-bb35-07d3-e335f1e28793", rpcResponse.Result.UUID);
            Assert.AreEqual("3839426635", rpcResponse.Result.Data.Entries[0].UserID);
            Assert.AreEqual("5.00000000000000000000", rpcResponse.Result.Data.Entries[0].Amount);
            Assert.AreEqual("-3.01", rpcResponse.Result.Data.Entries[1].Amount);
        }
    }
}
