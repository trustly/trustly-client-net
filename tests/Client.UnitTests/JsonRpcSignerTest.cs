using System;
using NUnit.Framework;
using Trustly.Api.Client;
using Trustly.Api.Domain;

namespace Trustly.Api.Client.UnitTests
{
    public class JsonRpcSignerTest
    {
        [Test]
        public void TestSerializingDeposit()
        {
            var factory = new JsonRpcFactory();
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

            var requestData = new DepositRequestData
            {
                NotificationURL = "localhost:1000",
                MessageID = "82bdbc09-7605-4265-b416-1e9549397edd",
                EndUserID = "127.0.0.1",

                Username = "team_ceres",
                Password = "a6e404c9-7ca8-1204-863d-5642e27c2747", // Is not the real password

                Attributes = new DepositRequestDataAttributes
                {
                    Amount = "100.00",
                    Currency = "SEK",
                    Country = "SE",
                    Locale = "sv_SE",
                    Firstname = "John",
                    Lastname = "Doe",
                    Email = "test@trustly.com",
                    SuccessURL = "https://google.com?q=success",
                    FailURL = "https://google.com?q=fail",
                    ShopperStatement = "Shop",
                }
            };

            var jsonRpcRequest = factory.Create<DepositRequestDataAttributes, DepositRequestData>(requestData, "Deposit", "e43f9dd4-e0ee-4c0a-9464-b962d590cfac");

            var serializer = new Serializer();

            var serialized = serializer.SerializeData(jsonRpcRequest.Params.Data, true);
            var expectedSerialized = "AttributesAmount100.00CountrySECurrencySEKEmailtest@trustly.comFailURLhttps://google.com?q=failFirstnameJohnLastnameDoeLocalesv_SEShopperStatementShopSuccessURLhttps://google.com?q=successEndUserID127.0.0.1MessageID82bdbc09-7605-4265-b416-1e9549397eddNotificationURLlocalhost:1000Passworda6e404c9-7ca8-1204-863d-5642e27c2747Usernameteam_ceres";

            Assert.That(serialized, Is.EqualTo(expectedSerialized));

            var signer = new JsonRpcSigner(serializer, settings);

            var plaintext = signer.CreatePlaintext(serialized, jsonRpcRequest.Method, jsonRpcRequest.Params.UUID);
            var expectedPlaintext = "Deposite43f9dd4-e0ee-4c0a-9464-b962d590cfacAttributesAmount100.00CountrySECurrencySEKEmailtest@trustly.comFailURLhttps://google.com?q=failFirstnameJohnLastnameDoeLocalesv_SEShopperStatementShopSuccessURLhttps://google.com?q=successEndUserID127.0.0.1MessageID82bdbc09-7605-4265-b416-1e9549397eddNotificationURLlocalhost:1000Passworda6e404c9-7ca8-1204-863d-5642e27c2747Usernameteam_ceres";

            Assert.That(plaintext, Is.EqualTo(expectedPlaintext));

            var actualSignature = signer.CreateSignature(jsonRpcRequest.Method, jsonRpcRequest.Params.UUID, jsonRpcRequest.Params.Data);

            var expectedSignature = "09zQ00rONzJK+j1Yc+q85SvKmQsGUT9uyysdsARPukhSRaFLmH84k/LW0hbW619GoV/DwAr3s/zQvFM5b8fT+SW9GX0Mf9OpMcK45RuEPlK+E2RBYPZhrKpb47RLeLmGMlI2dSmNc8kxotFh1zwQf2h8WWh1IGwmLobqn+Uun+AbC+uxi7PRkiqKBhRZtZlUPxyvUm05xd33RuB2uDrjEVeTQK9i99fO/EML/IviAQL4SebI6LfmfrP8HitkKvgcpjzVTCkkOwbb9spe/xLX8N/zRtdl7rUAAKBkyOtMuPoHjj1/F7BmPfTXnfAOBOeeWlEkDLcEzb8/agBH02H2GQ==";

            Assert.That(actualSignature, Is.EqualTo(expectedSignature));
        }
    }
}