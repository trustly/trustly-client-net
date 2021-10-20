using System;
using NUnit.Framework;
using Trustly.Api.Client;
using Trustly.Api.Domain.Requests;

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
                    Firstname = "John",
                    Lastname = "Doe",
                }
            };

            var jsonRpcRequest = factory.Create(requestData, "Deposit", "258a2184-2842-b485-25ca-293525152425");

            var serializer = new Serializer();

            var serialized = serializer.SerializeData(jsonRpcRequest.Params.Data);
            var expectedSerialized = "AttributesAmount100.00CountrySECurrencySEKFirstnameJohnLastnameDoeEndUserID127.0.0.1MessageID82bdbc09-7605-4265-b416-1e9549397eddNotificationURLlocalhost:1000Passworda6e404c9-7ca8-1204-863d-5642e27c2747Usernameteam_ceres";

            Assert.AreEqual(expectedSerialized, serialized);

            var signer = new JsonRpcSigner(serializer, settings);

            var plaintext = signer.CreatePlaintext(serialized, jsonRpcRequest.Method, jsonRpcRequest.Params.UUID);
            var expectedPlaintext = "Deposit258a2184-2842-b485-25ca-293525152425AttributesAmount100.00CountrySECurrencySEKFirstnameJohnLastnameDoeEndUserID127.0.0.1MessageID82bdbc09-7605-4265-b416-1e9549397eddNotificationURLlocalhost:1000Passworda6e404c9-7ca8-1204-863d-5642e27c2747Usernameteam_ceres";

            Assert.AreEqual(expectedPlaintext, plaintext);

            signer.Sign(jsonRpcRequest);

            var expectedSignature = "xRed4cLfZs2L5WoJVHiRFvD9yTTvbT0i/BgfhitnTvX7DpfmAz9cmGs3wcTpfYanGlW6hkY7zg7esuaGjPr3NvsWxLGLKBxw97oS7KDmp/FFPnrYulle4MsmKFH5jPB1HMZn2kybXO7a/v1QhVkyKgPGtGSznMBmR8iObbkBGjKbaHdpzwUR2HBK0bomwjIdG7Qx25UMTkMU8a9iNpvwXI71zO9/97DQJK3UiXCicJLNReOTtqcxWL/gUi9h/H7tK6M5kDeNtyRolOhznVZLX/rkFg7exhRvjPk8nEGjMJ3B1O4nEm/xFM0fh4uqfv8QyZrYEzX/K7cfNXflax4n0g==";

            Assert.AreEqual(expectedSignature, jsonRpcRequest.Params.Signature);
        }
    }
}