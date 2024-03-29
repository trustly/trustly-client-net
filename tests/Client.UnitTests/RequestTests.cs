using System;
using NUnit.Framework;
using Trustly.Api.Client;
using Trustly.Api.Domain.Exceptions;
using System.Threading.Tasks;
using Trustly.Api.Domain.Base;
using Newtonsoft.Json;

namespace Trustly.Api.Client.Tests
{
    public class RequestTests
    {
        private TrustlyApiClient client;

        [SetUp]
        public void SetUp()
        {
            this.client = new TrustlyApiClient(TrustlyApiClientSettings.ForDefaultTest());
        }

        [Test]
        public void TestAccountLedger()
        {
            var response = client.AccountLedger(new Trustly.Api.Domain.Requests.AccountLedgerRequestData
            {
                Currency = "SEK",
                FromDate = "2010-01-01 00:00:00",
                ToDate = "2021-01-01 00:00.00"
            });

            Assert.NotNull(response);
            Assert.NotNull(response.Entries);
        }

        private class FooAttributes : AbstractRequestParamsDataAttributes
        {
            public string NationalIdentificationNumber { get; set; }
        }

        private class FooRequestData : AbstractToTrustlyRequestParamsData<FooAttributes>
        {
            public string EndUserID { get; set; }
            public string ClearingHouse { get; set; }
        }

        private class FooResponse : AbstractResponseResultData
        {
            [JsonProperty("descriptor")]
            public string Descriptor { get; set; }
        }

        [Test]
        public void TestCustomInvalidFunction()
        {
            var ex = Assert.Throws<TrustlyDataException>(() =>
            {
                var response = client.SendRequest<FooRequestData, FooResponse>(new FooRequestData
                {
                    EndUserID = "123",
                    ClearingHouse = "CLRNGHS",
                    Attributes = new FooAttributes
                    {
                        NationalIdentificationNumber = "870604-6615"
                    }
                }, "Foo", Guid.NewGuid().ToString());
            });

            Assert.AreEqual("ERROR_INVALID_FUNCTION", ex.ResponseError.Message);
        }

        [Test]
        public void TestAccountPayout()
        {
            var ex = Assert.Throws<TrustlyDataException>(() =>
            {
                var response = client.AccountPayout(new Trustly.Api.Domain.Requests.AccountPayoutRequestData
                {
                    NotificationURL = "https://fake.test.notification.trustly.com",
                    MessageID = Guid.NewGuid().ToString(),
                    EndUserID = "pontus.eliason@trustly.com",
                    AccountID = "1234567890",
                    Currency = "SEK",
                    Amount = "100.1",

                    Attributes = new Trustly.Api.Domain.Requests.AccountPayoutRequestDataAttributes
                    {
                        ShopperStatement = "A Shopper Statement"
                    }
                });
            });

            Assert.AreEqual("ERROR_INVALID_BANK_ACCOUNT_NUMBER", ex.ResponseError.Message);
            Assert.AreEqual("ERROR_INVALID_BANK_ACCOUNT_NUMBER", ex.ResponseError.Error.Data.Message);
        }

        [Test]
        public void TestApproveWithdrawal()
        {
            var ex = Assert.Throws<TrustlyDataException>(() =>
            {
                var response = client.ApproveWithdrawal(new Trustly.Api.Domain.Requests.ApproveWithdrawalRequestData
                {
                    OrderID = 123_123
                });
            });

            Assert.AreEqual("ERROR_NOT_FOUND", ex.ResponseError.Message);
        }

        [Test]
        public void TestBalance()
        {
            var response = client.Balance(new Trustly.Api.Domain.Requests.BalanceRequestData
            {
            });

            Assert.NotNull(response);
            Assert.NotNull(response.Entries); // If not null, then something has been set, even if empty.

            // TODO: If empty, can we somehow do a deposit from the API, to simulate one?
        }

        [Test]
        public void TestCancelCharge()
        {
            var ex = Assert.Throws<TrustlyDataException>(() =>
            {
                var response = client.CancelCharge(new Trustly.Api.Domain.Requests.CancelChargeRequestData
                {
                    OrderId = "123123"
                });
            });

            Assert.AreEqual("ERROR_INVALID_ORDER_ID", ex.ResponseError.Message);
        }

        [Test]
        public void TestCharge()
        {
            var ex = Assert.Throws<TrustlyRejectionException>(() =>
            {
                var response = client.Charge(new Trustly.Api.Domain.Requests.ChargeRequestData
                {
                    NotificationURL = "https://fake.test.notification.trustly.com",

                    AccountID = "1234567890",
                    MessageID = Guid.NewGuid().ToString(),
                    Currency = "SEK",
                    Amount = "100.00",
                    EndUserID = "pontus.eliason@trustly.com",

                    Attributes = new Trustly.Api.Domain.Requests.ChargeRequestDataAttributes
                    {
                        Email = "pontus.eliason@trustly.com",
                        ShopperStatement = "A Shopper Statement"
                    }
                });
            });

            Assert.AreEqual("ERROR_ACCOUNT_NOT_FOUND", ex.Reason);
        }

        [Test]
        public void TestDenyWithdrawals()
        {
            var ex = Assert.Throws<TrustlyDataException>(() =>
            {
                var response = client.DenyWithdrawal(new Trustly.Api.Domain.Requests.DenyWithdrawalRequestData
                {
                    OrderID = 123_123
                });
            });

            Assert.AreEqual("ERROR_NOT_FOUND", ex.ResponseError.Message);
        }

        [Test]
        public void TestRegisterAccount()
        {
            var response = client.RegisterAccount(new Trustly.Api.Domain.Requests.RegisterAccountRequestData
            {
                EndUserID = "123123",
                ClearingHouse = "SWEDEN",
                BankNumber = "6112",
                AccountNumber = "69706212",
                Firstname = "Steve",
                Lastname = "Smith",
                Attributes = new Trustly.Api.Domain.Requests.RegisterAccountRequestDataAttributes
                {
                    DateOfBirth = "1979-01-31",
                    MobilePhone = "+46709876543",
                    NationalIdentificationNumber = "790131-1234",
                    AddressCountry = "SE",
                    AddressPostalCode = "SE-11253",
                    AddressCity = "Stockholm",
                    AddressLine1 = "Main street 1",
                    AddressLine2 = "Apartment 123",
                    Address = "Birgerstreet 14, SE-11411 Stockholm, Sweden",
                    Email = "test@trustly.com"
                }
            });

            Assert.NotNull(response);
            Assert.AreEqual(response.Descriptor, "**706212");
            Assert.AreEqual(response.ClearingHouse, "SWEDEN");
            Assert.AreEqual(response.Bank, "Handelsbanken");
        }

        [Test]
        public void TestRegisterAccountPayout()
        {
            var response = client.RegisterAccountPayout(new Trustly.Api.Domain.Requests.RegisterAccountPayoutRequestData
            {
                EndUserID = "123123",
                ClearingHouse = "SWEDEN",
                BankNumber = "6112",
                AccountNumber = "69706212",
                Firstname = "Steve",
                Lastname = "Smith",
                NotificationURL = "https://fake.test.notification.trustly.com",
                MessageID = Guid.NewGuid().ToString(),
                Currency = "SEK",
                Amount = "100.1",

                Attributes = new Trustly.Api.Domain.Requests.RegisterAccountPayoutRequestDataAttributes
                {
                    DateOfBirth = "1979-01-31",
                    MobilePhone = "+46709876543",
                    NationalIdentificationNumber = "790131-1234",
                    AddressCountry = "SE",
                    AddressPostalCode = "SE-11253",
                    AddressCity = "Stockholm",
                    AddressLine1 = "Main street 1",
                    AddressLine2 = "Apartment 123",
                    Address = "Birgerstreet 14, SE-11411 Stockholm, Sweden",
                    Email = "test@trustly.com",
                    ShopperStatement = "A Shopper Statement"
                }
            });

            Assert.NotNull(response);
            Assert.NotNull(response.OrderID);
        }


        [Test]
        public void TestDeposit()
        {
            var response = client.Deposit(new Trustly.Api.Domain.Requests.DepositRequestData
            {
                NotificationURL = "https://fake.test.notification.trustly.com",
                MessageID = Guid.NewGuid().ToString(),
                EndUserID = "pontus.eliason@trustly.com",
                Attributes = new Trustly.Api.Domain.Requests.DepositRequestDataAttributes
                {
                    Amount = "100.00",
                    Firstname = "John",
                    Lastname = "Doe",
                    Email = "pontus.eliason@trustly.com",
                    Currency = "EUR",
                    Country = "SE",
                    Locale = "sv_SE",
                    ShopperStatement = "Trustly Test Deposit"
                }
            });

            Assert.NotNull(response);
            Assert.IsFalse(string.IsNullOrEmpty(response.URL));
        }

        [Test]
        public void TestGetWithdrawals()
        {
            // GetWithdrawals seems to work even if the OrderID does not exist.
            var response = client.GetWithdrawals(new Trustly.Api.Domain.Requests.GetWithdrawalsRequestData
            {
                OrderID = "123123"
            });

            Assert.NotNull(response);
            Assert.NotNull(response.Entries); // If not null, at least something was set
        }

        [Test]
        public void TestRefund()
        {
            var ex = Assert.Throws<TrustlyDataException>(() =>
            {
                var response = client.Refund(new Trustly.Api.Domain.Requests.RefundRequestData
                {
                    OrderID = "123123",
                    Currency = "SEK",
                    Amount = "100.00",
                    Attributes = new Trustly.Api.Domain.Requests.RefundRequestDataAttributes
                    {
                        ExternalReference = "Reference" + new Random().Next()
                    }
                });
            });

            Assert.NotNull("ERROR_INVALID_ORDER_ID", ex.ResponseError.Message);
        }

        [Ignore("It gives ERROR_UNKNOWN if empty response is returned. Not trustworthy.")]
        [Test]
        public void TestSettlementReport()
        {
            var response = client.SettlementReport(new Trustly.Api.Domain.Requests.SettlementReportRequestData
            {
                Currency = "SEK",
                SettlementDate = "2020-01-01 00:00:00"
            });

            Assert.NotNull(response);
            Assert.NotNull(response.Entries); // If not null, something was set.
        }

        [Test]
        public void TestWithdraw()
        {
            var response = client.Withdraw(new Trustly.Api.Domain.Requests.WithdrawRequestData
            {
                NotificationURL = "https://fake.test.notification.trustly.com",
                MessageID = Guid.NewGuid().ToString(),
                EndUserID = "pontus.eliason@trustly.com",
                Currency = "SEK",

                Attributes = new Trustly.Api.Domain.Requests.WithdrawRequestDataAttributes
                {
                    SuggestedAmount = "100.00",
                    SuggestedMinAmount = "10.00",
                    SuggestedMaxAmount = "1000.00",
                    Firstname = "John",
                    Lastname = "Doe",
                    Email = "pontus.eliason@trustly.com",
                    Country = "SE",
                    Locale = "sv_SE",
                    ShopperStatement = "Trustly Test Deposit"
                }
            });

            Assert.NotNull(response);
            Assert.IsFalse(string.IsNullOrEmpty(response.URL));
        }
    }
}