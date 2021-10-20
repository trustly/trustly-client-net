using NUnit.Framework;
using Trustly.Api.Client;
using Trustly.Api.Domain.Base;
using Trustly.Api.Domain.Exceptions;
using Trustly.Api.Domain.Notifications;
using Trustly.Api.Domain.Requests;

namespace Trustly.Api.Client.UnitTests
{
    public class SerializerTest
    {
        [Test]
        public void TestSerializingDepositWithoutValidation()
        {
            var serializer = new Serializer();
            var factory = new JsonRpcFactory();

            var jsonRpc = factory.Create(new DepositRequestData
            {
                Username = "merchant_username",
                Password = "merchant_password",
                NotificationURL = "URL_to_your_notification_service",
                EndUserID = "12345",
                MessageID = "your_unique_deposit_id",
                Attributes = new DepositRequestDataAttributes
                {
                    Locale = "sv_SE",
                    Currency = "SEK",
                    IP = "123.123.123.123",
                    MobilePhone = "+46709876543",
                    Firstname = "John",
                    Lastname = "Doe",
                    NationalIdentificationNumber = "790131-1234"
                }
            }, "Deposit");

            var serialized = serializer.SerializeData(jsonRpc.Params.Data);
            var expected = "AttributesCurrencySEKFirstnameJohnIP123.123.123.123LastnameDoeLocalesv_SEMobilePhone+46709876543NationalIdentificationNumber790131-1234EndUserID12345MessageIDyour_unique_deposit_idNotificationURLURL_to_your_notification_servicePasswordmerchant_passwordUsernamemerchant_username";

            Assert.AreEqual(expected, serialized);
        }

        [Test]
        public void TestNullProperties()
        {
            var serializer = new Serializer();

            var testAssembly = typeof(JsonRpcSignerTest).Assembly;

            TrustlyApiClientSettings settings;
            using (var merchantPrivateKey = testAssembly.GetManifestResourceStream("Trustly.Api.Client.UnitTests.Keys.merchant_private_key.cer"))
            {
                using (var merchantPublicKey = testAssembly.GetManifestResourceStream("Trustly.Api.Client.UnitTests.Keys.merchant_public_key.cer"))
                {
                    settings = TrustlyApiClientSettings
                        .ForTest()
                        .WithCredentialsFromEnv()
                        .WithCertificatesFromStreams(merchantPublicKey, merchantPrivateKey)
                        .AndTrustlyCertificate();
                }
            }

            var client = new TrustlyApiClient(settings);
            var signer = new JsonRpcSigner(serializer, settings);

            var rpcResponse = client.CreateResponsePackage("account", "e76ffbe5-e0f9-4402-8689-f868ed2021f8", new NotificationResponse { Status = "OK" });

            var serialized = serializer.SerializeData(rpcResponse.GetData());

            Assert.AreEqual("statusOK", serialized);

            signer.Sign(rpcResponse);

            Assert.AreEqual(
                "J28IN0yXZN3dlV2ikg4nQKwnP98kso8lzpmuwBcfbXr8i3XeEyydRM4jRwsOOeF0ilGuXyr1Kyb3+1j4mVtgU0SwjVgBHWrYPMegNeykY3meto/aoATH0mvop4Ex1OKO7w/S/ktR2J0J5Npn/EuiKGiVy5GztHYTh9hWmZBCElYPZf4Rsd1CJQJAPlZeAuRcrb5dnbiGJvTEaL/7VLcPT27oqAUefSNb/zNt5yL+wH6BihlkpZ/mtE61lX5OpC46iql6hpsrlOBD3BroYfcwgk1t3YdcNOhVWrmkrlVptGQ/oy6T/LSIKbkG/tJsuV8sl6w1Z31IesK6MZDfSJbcXw==",
                rpcResponse.GetSignature()
            );
        }

        [Test]
        public void TestMissingDepositShopperStatement()
        {
            var serializer = new Serializer();
            var factory = new JsonRpcFactory();
            var validator = new JsonRpcValidator();

            var jsonRpc = factory.Create(new DepositRequestData
            {
                Username = "merchant_username",
                Password = "merchant_password",
                NotificationURL = "URL_to_your_notification_service",
                EndUserID = "12345",
                MessageID = "your_unique_deposit_id",
                Attributes = new DepositRequestDataAttributes
                {
                    Country = "SE",
                    Locale = "sv_SE",
                    Currency = "SEK",
                    IP = "123.123.123.123",
                    MobilePhone = "+46709876543",
                    Firstname = "John",
                    Lastname = "Doe",
                    NationalIdentificationNumber = "790131-1234"
                }
            }, "Deposit");

            Assert.Throws<TrustlyDataException>(() =>
            {
                validator.Validate(jsonRpc);
            });

            jsonRpc.Params.Data.Attributes.ShopperStatement = "A Statement";

            validator.Validate(jsonRpc);
        }
    }
}