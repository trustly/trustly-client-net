using System;
using NUnit.Framework;
using Trustly.Api.Domain.Exceptions;
using Newtonsoft.Json;
using System.Net;
using Trustly.Api.Domain;

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
            var response = client.AccountLedger(new AccountLedgerRequestData
            {
                Currency = "SEK",
                FromDate = "2010-01-01 00:00:00",
                ToDate = "2021-01-01 00:00.00"
            });

            Assert.That(response, Is.Not.Null);
        }

        private class FooAttributes : AbstractRequestDataAttributes
        {
            public string NationalIdentificationNumber { get; set; }
        }

        private class FooRequestData : AbstractRequestData<FooAttributes>
        {
            public string EndUserID { get; set; }
            public string ClearingHouse { get; set; }
            //public FooAttributes Attributes { get; set; }
        }

        private class FooResponseData //: AbstractResponseResult
        {
            [JsonProperty("descriptor")]
            public string Descriptor { get; set; }
        }

        [Test]
        public void TestCustomInvalidFunction()
        {
            var ex = Assert.Throws<TrustlyDataException>(() =>
            {
                var response = client.SendRequest<FooAttributes, FooRequestData, FooResponseData>(new FooRequestData
                {
                    EndUserID = "123",
                    ClearingHouse = "CLRNGHS",
                    Attributes = new FooAttributes
                    {
                        NationalIdentificationNumber = "870604-6615"
                    }
                }, "Foo", Guid.NewGuid().ToString());
            });

            Assert.That(ex.ResponseError.Message, Is.EqualTo("ERROR_INVALID_FUNCTION"));
        }

        [Test]
        public void TestAccountPayout()
        {
            var ex = Assert.Throws<TrustlyDataException>(() =>
            {
                var response = client.AccountPayout(new AccountPayoutRequestData
                {
                    NotificationURL = "https://fake.test.notification.trustly.com",
                    MessageID = Guid.NewGuid().ToString(),
                    EndUserID = "pontus.eliason@trustly.com",
                    AccountID = "1234567890",
                    Currency = "SEK",
                    Amount = "100.1",

                    Attributes = new AccountPayoutRequestDataAttributes
                    {
                        ShopperStatement = "A Shopper Statement"
                    }
                });
            });

            Assert.That(ex.ResponseError.Message, Is.EqualTo("ERROR_INVALID_BANK_ACCOUNT_NUMBER"));
            Assert.That(ex.ResponseError.Error.Data.AdditionalProperties["message"].ToString(), Is.EqualTo("ERROR_INVALID_BANK_ACCOUNT_NUMBER"));
        }

        [Test]
        public void TestApproveWithdrawal()
        {
            var ex = Assert.Throws<TrustlyDataException>(() =>
            {
                var response = client.ApproveWithdrawal(new ApproveWithdrawalRequestData
                {
                    OrderID = 123_123
                });
            });

            Assert.That(ex.ResponseError.Message, Is.EqualTo("ERROR_NOT_FOUND"));
        }

        [Test]
        public void TestBalance()
        {
            var response = client.Balance(new BalanceRequestData
            {
            });

            Assert.That(response, Is.Not.Null);

            // TODO: If empty, can we somehow do a deposit from the API, to simulate one?
        }

        [Test]
        public void TestCancelCharge()
        {
            var ex = Assert.Throws<TrustlyDataException>(() =>
            {
                var response = client.CancelCharge(new CancelChargeRequestData
                {
                    OrderID = "123123"
                });
            });

            Assert.That(ex.ResponseError.Message, Is.EqualTo("ERROR_INVALID_ORDER_ID"));
        }

        [Test]
        public void TestCharge()
        {
            var ex = Assert.Throws<TrustlyRejectionException>(() =>
            {
                var response = client.Charge(new ChargeRequestData
                {
                    NotificationURL = "https://fake.test.notification.trustly.com",

                    AccountID = "1234567890",
                    MessageID = Guid.NewGuid().ToString(),
                    Currency = "SEK",
                    Amount = "100.00",
                    EndUserID = "pontus.eliason@trustly.com",

                    Attributes = new ChargeRequestDataAttributes
                    {
                        Email = "pontus.eliason@trustly.com",
                        ShopperStatement = "A Shopper Statement"
                    }
                });
            });

            Assert.That(ex.Reason, Is.EqualTo("ERROR_ACCOUNT_NOT_FOUND"));
        }

        [Test]
        public void TestDenyWithdrawals()
        {
            var ex = Assert.Throws<TrustlyDataException>(() =>
            {
                var response = client.DenyWithdrawal(new DenyWithdrawalRequestData
                {
                    OrderID = 123_123
                });
            });

            Assert.That(ex.ResponseError.Message, Is.EqualTo("ERROR_NOT_FOUND"));
        }

        [Test]
        public void TestRegisterAccount()
        {
            var response = client.RegisterAccount(new RegisterAccountRequestData
            {
                EndUserID = "123123",
                ClearingHouse = "SWEDEN",
                BankNumber = "6112",
                AccountNumber = "69706212",
                Firstname = "Steve",
                Lastname = "Smith",
                Attributes = new RegisterAccountRequestDataAttributes
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

            Assert.That(response, Is.Not.Null);
            Assert.That("**706212", Is.EqualTo(response.Descriptor));
            Assert.That("SWEDEN", Is.EqualTo(response.ClearingHouse));
            Assert.That("Handelsbanken", Is.EqualTo(response.Bank));
        }

        [Test]
        public void TestRegisterAccountPayout()
        {
            var response = client.RegisterAccountPayout(new RegisterAccountPayoutRequestData
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

                Attributes = new RegisterAccountPayoutRequestDataAttributes
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

            Assert.That(response, Is.Not.Null);
            Assert.That(response.OrderID, Is.Not.Null);
        }

        [Test]
        public void TestDeposit()
        {
            var response = client.Deposit(new DepositRequestData
            {
                NotificationURL = "https://fake.test.notification.trustly.com",
                MessageID = Guid.NewGuid().ToString(),
                EndUserID = "pontus.eliason@trustly.com",
                Attributes = new DepositRequestDataAttributes
                {
                    Amount = "100.00",
                    Firstname = "John",
                    Lastname = "Doe",
                    Email = "pontus.eliason@trustly.com",
                    Currency = "EUR",
                    Country = "SE",
                    Locale = "sv_SE",
                    ShopperStatement = "Trustly Test Deposit",
                    SuccessURL = "https://google.com?q=success",
                    FailURL = "https://google.com?q=fail",
                }
            });

            Assert.That(response, Is.Not.Null);
            Assert.That(response.URL, Is.Not.Empty);
        }

        [Test]
        public void TestDepositWithCustomProxyClient()
        {
            var callCount = 0;
            var proxyClient = new TrustlyApiClient(TrustlyApiClientSettings.ForDefaultTest())
            {
                RequestCreator = url =>
                {
                    callCount++;
                    var request = WebRequest.Create(url);
                    request.Proxy = new WebProxy();

                    return request;
                }
            };

            var response = proxyClient.Deposit(new DepositRequestData
            {
                NotificationURL = "https://fake.test.notification.trustly.com",
                MessageID = Guid.NewGuid().ToString(),
                EndUserID = "pontus.eliason@trustly.com",
                Attributes = new DepositRequestDataAttributes
                {
                    Amount = "100.00",
                    Firstname = "John",
                    Lastname = "Doe",
                    Email = "pontus.eliason@trustly.com",
                    Currency = "EUR",
                    Country = "SE",
                    Locale = "sv_SE",
                    ShopperStatement = "Trustly Test Deposit",
                    SuccessURL = "https://google.com?q=success",
                    FailURL = "https://google.com?q=fail",
                }
            });

            Assert.That(response, Is.Not.Null);
            Assert.That(response.URL, Is.Not.Empty);
            Assert.That(callCount, Is.EqualTo(1));
        }

        [Test]
        public void TestGetWithdrawals()
        {
            // GetWithdrawals seems to work even if the OrderID does not exist.
            var response = client.GetWithdrawals(new GetWithdrawalsRequestData
            {
                OrderID = 123123
            });

            Assert.That(response, Is.Not.Null);
        }

        [Test]
        public void TestRefund()
        {
            var ex = Assert.Throws<TrustlyDataException>(() =>
            {
                var response = client.Refund(new RefundRequestData
                {
                    OrderID = "123123",
                    Currency = "SEK",
                    Amount = "100.00",
                    Attributes = new RefundRequestDataAttributes
                    {
                        ExternalReference = "Reference" + new Random().Next()
                    }
                });
            });

            Assert.That(ex.ResponseError.Message, Is.EqualTo("ERROR_INVALID_ORDER_ID"));
        }

        [Ignore("It gives ERROR_UNKNOWN if empty response is returned. Not trustworthy.")]
        [Test]
        public void TestSettlementReport()
        {
            var response = client.SettlementReport(new SettlementReportRequestData
            {
                Currency = "SEK",
                SettlementDate = "2020-01-01 00:00:00"
            });

            Assert.That(response, Is.Not.Null);
        }

        [Test]
        public void TestWithdraw()
        {
            var response = client.Withdraw(new WithdrawRequestData
            {
                NotificationURL = "https://fake.test.notification.trustly.com",
                MessageID = Guid.NewGuid().ToString(),
                EndUserID = "pontus.eliason@trustly.com",
                Currency = "SEK",

                Attributes = new WithdrawRequestDataAttributes
                {
                    SuggestedAmount = "100.00",
                    SuggestedMinAmount = "10.00",
                    SuggestedMaxAmount = "1000.00",
                    Firstname = "John",
                    Lastname = "Doe",
                    Email = "pontus.eliason@trustly.com",
                    Country = "SE",
                    Locale = "sv_SE",
                    ShopperStatement = "Trustly Test Deposit",
                    SuccessURL = "https://google.com?q=success",
                    FailURL = "https://google.com?q=fail",
                }
            });

            Assert.That(response, Is.Not.Null);
            Assert.That(response.URL, Is.Not.Empty);
        }
    }
}