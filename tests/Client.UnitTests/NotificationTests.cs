using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Trustly.Api.Client;
using Trustly.Api.Domain.Exceptions;
using Trustly.Api.Domain.Notifications;

namespace Client.Tests
{
    public class NotificationTests
    {
        private TrustlyApiClient client;

        [SetUp]
        public void SetUp()
        {
            var testAssembly = typeof(NotificationTests).Assembly;

            TrustlyApiClientSettings settings;
            using (var trustlyPrivateKey = testAssembly.GetManifestResourceStream("Trustly.Api.Client.UnitTests.Keys.trustly_local_fake_private.pem"))
            {
                using (var trustlyPublicKey = testAssembly.GetManifestResourceStream("Trustly.Api.Client.UnitTests.Keys.trustly_local_fake_public.pem"))
                {
                    using (var merchantPublicKey = testAssembly.GetManifestResourceStream("Trustly.Api.Client.UnitTests.Keys.merchant_public_key.cer"))
                    {
                        // An ugly way to fake as if the "client" is the Trustly server. So we sign the "request" (notification) with Trustly private key.
                        // It is then validated by the same client, with the Trustly public key, as if it is on the other side of the communication.
                        settings = TrustlyApiClientSettings
                            .ForTest()
                            .WithoutCredentials()
                            .WithCertificatesFromStreams(merchantPublicKey, trustlyPrivateKey)
                            .AndTrustlyCertificateFromStream(trustlyPublicKey);
                    }
                }
            }

            this.client = new TrustlyApiClient(settings);
        }

        [Test]
        public void TestNotificationHandlerFromRequest()
        {
            var receivedDebitNotifications = 0;
            client.OnDebit += (sender, args) =>
            {
                receivedDebitNotifications++;
            };

            var mockRequest = this.CreateMockDepositNotificationRequest();
            client.HandleNotificationFromRequest(mockRequest.Object);

            Assert.AreEqual(1, receivedDebitNotifications);
        }

        [Test]
        public async Task TestNotificationHandlerFromMiddlewareRequest()
        {
            var receivedDebitNotifications = 0;
            client.OnDebit += (sender, args) =>
            {
                receivedDebitNotifications++;
                args.RespondWithOK();
            };

            var mockRequest = this.CreateMockDepositNotificationRequest();
            var mockHttpContext = new Mock<HttpContext>();
            var mockResponse = new Mock<HttpResponse>();

            mockResponse.SetupAllProperties();

            mockHttpContext.Setup(x => x.Request).Returns(() => mockRequest.Object);
            mockHttpContext.Setup(x => x.Response).Returns(() => mockResponse.Object);

            await TrustlyApiClientExtensions.HandleNotificationRequest(mockHttpContext.Object, null);

            Assert.AreEqual(1, receivedDebitNotifications);
            Assert.AreEqual(200, mockResponse.Object.StatusCode);
        }

        [Test]
        public async Task TestNotificationHandlerFromMiddlewareRequestWithErrorResponse()
        {
            var receivedDebitNotifications = 0;
            client.OnDebit += (sender, args) =>
            {
                receivedDebitNotifications++;
                args.RespondWithFailed("Things went badly");
            };

            var mockRequest = this.CreateMockDepositNotificationRequest();
            var mockHttpContext = new Mock<HttpContext>();
            var mockResponse = new Mock<HttpResponse>();

            mockResponse.SetupAllProperties();

            mockHttpContext.Setup(x => x.Request).Returns(() => mockRequest.Object);
            mockHttpContext.Setup(x => x.Response).Returns(() => mockResponse.Object);

            await TrustlyApiClientExtensions.HandleNotificationRequest(mockHttpContext.Object, null);

            Assert.AreEqual(1, receivedDebitNotifications);
            Assert.AreEqual(500, mockResponse.Object.StatusCode);

            using (var sr = new StreamReader(mockResponse.Object.Body))
            {
                var bodyString = sr.ReadToEnd();

                Assert.IsTrue(bodyString.Contains("Things went badly"));
            }
        }

        [Test]
        public void TestNotificationHandlerFromMiddlewareRequestWithoutListener()
        {
            var mockRequest = CreateMockDepositNotificationRequest();
            var mockHttpContext = new Mock<HttpContext>();
            var mockResponse = new Mock<HttpResponse>();

            mockResponse.SetupAllProperties();

            mockHttpContext.Setup(x => x.Request).Returns(() => mockRequest.Object);
            mockHttpContext.Setup(x => x.Response).Returns(() => mockResponse.Object);

            Assert.ThrowsAsync<TrustlyNoNotificationListenerException>(async () =>
            {
                await TrustlyApiClientExtensions.HandleNotificationRequest(mockHttpContext.Object, null);
            });
        }

        [Test]
        public void TestNotificationHandlerFromRequestWithWrongHttpMethod()
        {
            var receivedDebitNotifications = 0;
            client.OnDebit += (sender, args) =>
            {
                receivedDebitNotifications++;
            };

            var mockRequest = this.CreateMockDepositNotificationRequest(method: "GET");

            Assert.Throws<TrustlyNotificationException>(() =>
            {
                client.HandleNotificationFromRequest(mockRequest.Object);
            });

            Assert.AreEqual(0, receivedDebitNotifications);
        }

        [Test]
        public void TestUnknownNotification()
        {
            var receivedDebitNotifications = 0;
            var receivedUnknownNotifications = 0;

            client.OnDebit += (sender, args) =>
            {
                receivedDebitNotifications++;
            };

            client.OnUnknownNotification += (sender, args) =>
            {
                receivedUnknownNotifications++;

                Assert.IsFalse(args.Data.ExtensionData.ContainsKey("Amount"));
                Assert.IsFalse(args.Data.ExtensionData.ContainsKey("EnduserID"));

                Assert.AreEqual("100.00", args.Data.ExtensionData["amount"]);
                Assert.AreEqual("user@email.com", args.Data.ExtensionData["enduserid"]);
            };

            var mockRequest = this.CreateMockDepositNotificationRequest(rpcMethod: "blaha");
            client.HandleNotificationFromRequest(mockRequest.Object);

            Assert.AreEqual(0, receivedDebitNotifications);
            Assert.AreEqual(1, receivedUnknownNotifications);
        }

        private Mock<HttpRequest> CreateMockDepositNotificationRequest(string method = "POST", string rpcMethod = "debit")
        {
            Mock<HttpRequest> mockRequest = new Mock<HttpRequest>();

            mockRequest.SetupAllProperties();

            mockRequest.Setup(x => x.Body).Returns(() =>
            {
                var json = JsonConvert.SerializeObject(
                    client.CreateRequestPackage(
                        new DebitNotificationData
                        {
                            Amount = "100.00",
                            Currency = "EUR",
                            EnduserID = "user@email.com",
                            MessageID = Guid.NewGuid().ToString(),
                            OrderID = Guid.NewGuid().ToString(),
                            NotificationID = Guid.NewGuid().ToString(),
                            Timestamp = "2021-01-01 01:01:01"
                        },
                        rpcMethod
                    )
                );

                var byteArray = Encoding.UTF8.GetBytes(json);
                var stream = new MemoryStream(byteArray);
                stream.Flush();
                stream.Position = 0;

                return stream;
            });
            mockRequest.Setup(x => x.Method).Returns(() => method);
            mockRequest.Setup(x => x.Path).Returns("/trustly/notifications");

            return mockRequest;
        }
    }
}