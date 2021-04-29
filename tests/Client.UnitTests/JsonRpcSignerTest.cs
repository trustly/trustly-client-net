using System;
using NUnit.Framework;
using Trustly.Api.Client;
using Trustly.Api.Domain.Requests;

namespace Trustly.Api.Domain.UnitTests
{
    public class JsonRpcSignerTest
    {
        [Test]
        public void TestSerializingDeposit()
        {
            var serializer = new Serializer();
            var factory = new JsonRpcFactory();
            var testAssembly = typeof(JsonRpcSignerTest).Assembly;

            TrustlyApiClientSettings settings;
            using (var merchantPrivateKey = testAssembly.GetManifestResourceStream("Trustly.Api.Client.UnitTests.Keys.merchant_private_key.cer"))
            {
                using (var merchantPublicKey = testAssembly.GetManifestResourceStream("Trustly.Api.Client.UnitTests.Keys.merchant_public_key.cer"))
                {
                    settings = new TrustlyApiClientSettings(true).WithKeysFromStreams(merchantPublicKey, merchantPrivateKey);
                }
            }

            var signer = new JsonRpcSigner(serializer, settings);

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
                    Country = "SE",
                    Firstname = "John",
                    Lastname = "Doe",
                }
            };

            var jsonRpcRequest = factory.Create(requestData, "Deposit", "258a2184-2842-b485-25ca-293525152425");

            var serialized = serializer.SerializeData(jsonRpcRequest.Params.Data);
            var expectedSerialized = "AttributesAmount100.00CountrySEFirstnameJohnLastnameDoeEndUserID127.0.0.1MessageID82bdbc09-7605-4265-b416-1e9549397eddNotificationURLlocalhost:1000Passworda6e404c9-7ca8-1204-863d-5642e27c2747Usernameteam_ceres";

            Assert.AreEqual(expectedSerialized, serialized);

            var plaintext = signer.CreatePlaintext(serialized, jsonRpcRequest.Method, jsonRpcRequest.Params.UUID);
            var expectedPlaintext = "Deposit258a2184-2842-b485-25ca-293525152425AttributesAmount100.00CountrySEFirstnameJohnLastnameDoeEndUserID127.0.0.1MessageID82bdbc09-7605-4265-b416-1e9549397eddNotificationURLlocalhost:1000Passworda6e404c9-7ca8-1204-863d-5642e27c2747Usernameteam_ceres";

            Assert.AreEqual(expectedPlaintext, plaintext);

            signer.Sign(jsonRpcRequest);

            var expectedSignature = "p5kZihrxRIk7BYrUY0PsXcjhej/PJIo9TRlIR+SSBcMa1z9egJEhcvo9IJRkGbONJ0q9sWAbu2mc2qcHz0zDeP3KUQ41tTyykSMQpEoxEx/UtVWAJFB8g1XYbTLFKO6JMSKJFvOugrYwCEchRfs1dlayc0KY/y/vq6MP2EPAtgW7ZAdSbJryt+BgN42Bri0mdS+8H37KOKAZoOy7H14IwjbekH8qwQOzR9HhkTs3o5avYuKii/yCeAhm8U15ACmYMbIPPshQZ0vaUU9Q+EzAgTIaYbGslzHCWbq2DwdshmP2kSSJIGrw/ZSd2CjHdfTz2BWnKYsdQ/uYyAuidWZv+g==";

            Assert.AreEqual(expectedSignature, jsonRpcRequest.Params.Signature);
        }
    }
}