using System;
using NUnit.Framework;

namespace Trustly.Api.Client.UnitTests
{
    public class SettlementReportParserTest
    {
        [Test]
        public void TestParser()
        {
            var parser = new SettlementReportParser();

            var csv =
                "datestamp,accountname,currency,amount,total,orderid,ordertype,messageid,username,fxpaymentamount,fxpaymentcurrency,settlementbankwithdrawalid,extraref\n" +
                "\"2018-11-16 12:52:22.293626+00\",SUSPENSE_ACCOUNT_CLIENT_FUNDS_FINLAND_OKOY,EUR,100.00,145.00,1288208729,Deposit,9567705,merchant1,,,1434179572,\n" +
                "\"2018-11-16 12:52:22.293626+00\",TRANSACTION_FEE_BANK_DEPOSIT,EUR,-1.00,145.00,1288208729,\"Deposit Fee\",9567705,merchant1,,,1434179572,\n" +
                "\"2018-11-16 12:53:21.019497+00\",BANK_WITHDRAWAL_QUEUED,EUR,-100.00,145.00,1288208729,Refund,\"Refund 2018-11-16 13:53:21.019497+01 9567705\",merchant1,,,1434179572,\n" +
                "\"2018-11-16 12:53:21.019497+00\",TRANSACTION_FEE_BANK_WITHDRAWAL,EUR,-1.00,145.00,1288208729,\"Refund Fee\",\"Refund 2018-11-16 13:53:21.019497+01 9567705\",merchant1,,,1434179572,\n" +
                "\"2018-11-16 12:02:43.235847+00\",BANK_WITHDRAWAL_QUEUED,EUR,-100.00,145.00,1134212451,AccountPayout,275852136,merchant1,,,1434179572,\n" +
                "\"2018-11-16 12:02:43.235847+00\",TRANSACTION_FEE_BANK_WITHDRAWAL,EUR,-1.00,145.00,1134212451,\"AccountPayout Fee\",275852136,merchant1,,,1434179572,\n" +
                "\"2018-11-16 11:04:01.702755+00\",TRANSACTION_FEE_BANK_DEPOSIT,EUR,-1.00,145.00,2590840341,\"Deposit Fee\",1560785,merchant1,,,1434179572,\n" +
                "\"2018-11-16 11:04:01.702755+00\",SUSPENSE_ACCOUNT_CLIENT_FUNDS_SWEDEN_SWED,EUR,150.00,145.00,2590840341,Deposit,1560785,merchant1,1500.00,SEK,1434179572,\n" +
                "\"2018-11-16 10:48:19.142018+00\",FOREIGN_EXCHANGE_SPREAD,EUR,100.00,145.00,3061625784,FX,f6ee4ec7-3bb7-4182-8368-317b4ea28cc2,merchant1,1000,SEK,1434179572,\n" +
                "\"2018-11-16 05:30:43.235847+00\",TRANSACTION_FEE_BANK_WITHDRAWAL,EUR,-1.00,145.00,,\"Settlement Fee\",\"Automatic EUR settlement 83942 for 1231459251 on 2018-11-16 05:30:43.225447+01  \",merchant1,,,1434179572,someref\n";

            var rows = parser.Parse(csv);

            Assert.AreEqual(10, rows.Count);
            Assert.AreEqual("SUSPENSE_ACCOUNT_CLIENT_FUNDS_FINLAND_OKOY", rows[0].AccountName);
            Assert.AreEqual("TRANSACTION_FEE_BANK_WITHDRAWAL", rows[9].AccountName);

            Assert.AreEqual("1434179572", rows[0].SettlementBankWithdrawalID);
            Assert.AreEqual("1434179572", rows[9].SettlementBankWithdrawalID);

            Assert.AreEqual(null, rows[0].ExternalReference);
            Assert.AreEqual("someref", rows[9].ExternalReference);
        }
    }
}
