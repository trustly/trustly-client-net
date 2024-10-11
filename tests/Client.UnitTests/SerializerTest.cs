using Newtonsoft.Json;
using NUnit.Framework;
using Trustly.Api.Client;
using Trustly.Api.Domain;
using Trustly.Api.Domain.Exceptions;
using Trustly.Api.Domain.Notifications;

namespace Trustly.Api.Client.UnitTests
{
    public class SerializerTest
    {
        [Test]
        public void TestSerializingDepositWithoutValidation()
        {
            var serializer = new Serializer();
            var factory = new JsonRpcFactory();

            var jsonRpc = factory.Create<DepositRequestDataAttributes, DepositRequestData>(new DepositRequestData
            {
                Username = "merchant_username",
                Password = "merchant_password",
                NotificationURL = "URL_to_your_notification_service",
                EndUserID = "12345",
                MessageID = "your_unique_deposit_id",
                Attributes = new DepositRequestDataAttributes
                {
                    Locale = "sv_SE",
                    Country = "sv",
                    Currency = "SEK",
                    IP = "123.123.123.123",
                    MobilePhone = "+46709876543",
                    Email = "test@trustly.com",
                    Firstname = "John",
                    Lastname = "Doe",
                    NationalIdentificationNumber = "790131-1234",
                    SuccessURL = "https://google.com/?q=success",
                    FailURL = "https://google.com/?q=fail"
                }
            }, "Deposit");

            var serialized = serializer.SerializeData(jsonRpc.Params.Data, true);
            var expected = "AttributesCountrysvCurrencySEKEmailtest@trustly.comFailURLhttps://google.com/?q=failFirstnameJohnIP123.123.123.123LastnameDoeLocalesv_SEMobilePhone+46709876543NationalIdentificationNumber790131-1234SuccessURLhttps://google.com/?q=successEndUserID12345MessageIDyour_unique_deposit_idNotificationURLURL_to_your_notification_servicePasswordmerchant_passwordUsernamemerchant_username";

            Assert.That(serialized, Is.EqualTo(expected));
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

            var rpcResponse = client.CreateResponsePackage(new AckData() { Status = AckDataStatus.OK }, "account", "e76ffbe5-e0f9-4402-8689-f868ed2021f8");

            var serialized = serializer.SerializeData(rpcResponse.Result.Data);

            Assert.That(serialized, Is.EqualTo("statusOK"));

            var actualSignature = signer.CreateSignature(
                rpcResponse.Result.Method,
                rpcResponse.Result.UUID,
                rpcResponse.Result.Data
            );

            Assert.That(actualSignature, Is.EqualTo(
                "J28IN0yXZN3dlV2ikg4nQKwnP98kso8lzpmuwBcfbXr8i3XeEyydRM4jRwsOOeF0ilGuXyr1Kyb3+1j4mVtgU0SwjVgBHWrYPMegNeykY3meto/aoATH0mvop4Ex1OKO7w/S/ktR2J0J5Npn/EuiKGiVy5GztHYTh9hWmZBCElYPZf4Rsd1CJQJAPlZeAuRcrb5dnbiGJvTEaL/7VLcPT27oqAUefSNb/zNt5yL+wH6BihlkpZ/mtE61lX5OpC46iql6hpsrlOBD3BroYfcwgk1t3YdcNOhVWrmkrlVptGQ/oy6T/LSIKbkG/tJsuV8sl6w1Z31IesK6MZDfSJbcXw=="   
            ));
        }

        [Test]
        public void TestMissingDepositShopperStatement()
        {
            var serializer = new Serializer();
            var factory = new JsonRpcFactory();
            var validator = new JsonRpcValidator();

            var jsonRpc = factory.Create<DepositRequestDataAttributes, DepositRequestData>(new DepositRequestData
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
                    NationalIdentificationNumber = "790131-1234",
                    Email = "test@trustly.com",
                    SuccessURL = "https://google.com/?q=success",
                    FailURL = "https://google.com/?q=fail"
                }
            }, "Deposit");
            jsonRpc.Params.Signature = "FakeSignature";

            Assert.Throws<TrustlyDataException>(() =>
            {
                validator.Validate(jsonRpc);
            });

            jsonRpc.Params.Data.Attributes.ShopperStatement = "A Statement";

            validator.Validate(jsonRpc);
        }

        public class UrlTargetWrapper
        {
            [JsonProperty("target")]
            public UrlTarget Target { get; set; }
        }

        [Test]
        public void TestUrlTarget()
        {
            var json = "{\"target\":\"_self\"}";
            var obj = JsonConvert.DeserializeObject<UrlTargetWrapper>(json);

            Assert.That(obj.Target, Is.EqualTo(UrlTarget.SELF));

            var backToJson = JsonConvert.SerializeObject(obj);

            Assert.That(backToJson, Is.EqualTo(json));
        }
    }
}