using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Trustly.Api.Domain.Base;
using Trustly.Api.Domain.Exceptions;
using Trustly.Api.Domain.Notifications;

namespace Trustly.Api.Client.Tests
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

        [TearDown]
        public void TearDown()
        {
            this.client.Dispose();
        }

        [Test]
        public async Task TestNotificationHandlerFromRequest()
        {
            var receivedDebitNotifications = 0;
            client.OnDebit += (sender, args) =>
            {
                receivedDebitNotifications++;
            };

            var mockRequest = this.CreateMockDebitNotificationRequest();
            await client.HandleNotificationFromRequestAsync(mockRequest.Object);

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

            var mockRequest = this.CreateMockDebitNotificationRequest();
            var mockHttpContext = new Mock<HttpContext>();
            var mockResponse = new Mock<HttpResponse>();

            mockResponse.SetupAllProperties();
            mockResponse.Setup(r => r.Headers).Returns(new HeaderDictionary());

            mockHttpContext.Setup(x => x.Request).Returns(mockRequest.Object);
            mockHttpContext.Setup(x => x.Response).Returns(mockResponse.Object);

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

            var mockHttpContext = new DefaultHttpContext();
            this.SetHttpRequestProperties(mockHttpContext.Request);
            mockHttpContext.Response.Body = new MemoryStream();

            await TrustlyApiClientExtensions.HandleNotificationRequest(mockHttpContext, null);

            Assert.AreEqual(1, receivedDebitNotifications);
            Assert.AreEqual(500, mockHttpContext.Response.StatusCode);

            mockHttpContext.Response.Body.Position = 0;
            using (var sr = new StreamReader(mockHttpContext.Response.Body))
            {
                var bodyString = sr.ReadToEnd();
                Assert.IsTrue(bodyString.Contains("Things went badly"));
            }
        }

        [Test]
        public void TestNotificationHandlerFromMiddlewareRequestWithoutListener()
        {
            var mockRequest = CreateMockDebitNotificationRequest();
            var mockHttpContext = new Mock<HttpContext>();
            var mockResponse = new Mock<HttpResponse>();

            mockResponse.SetupAllProperties();
            mockResponse.Setup(r => r.Headers).Returns(new HeaderDictionary());

            mockHttpContext.Setup(x => x.Request).Returns(mockRequest.Object);
            mockHttpContext.Setup(x => x.Response).Returns(mockResponse.Object);

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

            var mockRequest = this.CreateMockDebitNotificationRequest(method: "GET");

            Assert.ThrowsAsync<TrustlyNotificationException>(async () =>
            {
                await client.HandleNotificationFromRequestAsync(mockRequest.Object);
            });

            Assert.AreEqual(0, receivedDebitNotifications);
        }

        [Test]
        public async Task TestUnknownNotification()
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

            var mockRequest = this.CreateMockDebitNotificationRequest(rpcMethod: "blaha");
            await client.HandleNotificationFromRequestAsync(mockRequest.Object);

            Assert.AreEqual(0, receivedDebitNotifications);
            Assert.AreEqual(1, receivedUnknownNotifications);
        }

        [Test]
        public async Task TestAccountNotification()
        {
            // TODO: It seems you cannot validate the signature of Account Notification. Needs to be fixed.

            var receivedCount = 0;

            client.OnAccount += (sender, args) =>
            {
                receivedCount++;
            };

            var mockRequest = this.CreateMockAccountNotificationRequest();
            await client.HandleNotificationFromRequestAsync(mockRequest.Object);

            Assert.AreEqual(1, receivedCount);
        }

        [Test]
        public void TestExpectedNotificationSerialization()
        {
            var requestJson = ""
                + "{\n"
                + "    \"method\": \"account\",\n"
                + "    \"version\": \"1.1\",\n"
                + "    \"params\": {\n"
                //     Incorrect signature
                + "        \"signature\": \"qP1KOXErOUguNVPtGhmvv2SxQzRUOOdaM7oH6Oh+T6V4jKXyScyIQLb7FD4iSmIPXnWLv5X8TmeOBCnJtsdDmW7bcueK65VVCzIjs88HxJC37axgTOWBn8T4EDOOuRnC0MeKJFEDf5mDRXg/iRSJOsYg2JC5WFi4Ht4imuxAogYzqrYoCeuy2Vcn0fkD2oOGac6zVlkX6Q4Z+lAE36jxU+mvcTULHK7tBbpBfebN7I8tSm4xMjcZUF+F3h2Gkb3rQwkgx7GDf0wuPmIJ8uW5dfuDVHsoOPl3VqEln/NZX2L/KhjzDhnl6vNHil1iqWZQLtu4nErNA9sUwU87nPsl+w==\",\n"
                + "        \"uuid\": \"be7e6b93-13b9-4b8f-89e3-0ad8258db94c\",\n"
                + "        \"data\": {\n"
                + "            \"orderid\": \"7520047953\",\n"
                + "            \"verified\": \"0\",\n"
                + "            \"accountid\": \"4052851907\",\n"
                + "            \"messageid\": \"100137003A703263176\",\n"
                + "            \"attributes\": {\n"
                + "              \"bank\": \"Commerzbank\",\n"
                + "              \"descriptor\": \"****************441300\",\n"
                + "              \"lastdigits\": \"441300\",\n"
                + "              \"clearinghouse\": \"GERMANY\"\n"
                + "            }\n"
                + "        }\n"
                + "    }\n"
                + "}";
            var expectedSerialization = "accountid4052851907attributesbankCommerzbankclearinghouseGERMANYdescriptor****************441300lastdigits441300messageid100137003A703263176notificationidorderid7520047953verified0";

            var rpcRequest = JsonConvert.DeserializeObject<JsonRpcRequest<AccountNotificationData>>(requestJson);

            var serializer = new Serializer();
            var serializedData = serializer.SerializeData(rpcRequest.Params.Data);

            Assert.AreEqual(expectedSerialization, serializedData);
        }

        [Test]
        public void TestExpectedNotificationSerializationWithEmptyAttributes()
        {
            var requestJson = ""
                + "{\n"
                + "    \"method\": \"account\",\n"
                + "    \"version\": \"1.1\",\n"
                + "    \"params\": {\n"
                //     Incorrect signature
                + "        \"signature\": \"qP1KOXErOUguNVPtGhmvv2SxQzRUOOdaM7oH6Oh+T6V4jKXyScyIQLb7FD4iSmIPXnWLv5X8TmeOBCnJtsdDmW7bcueK65VVCzIjs88HxJC37axgTOWBn8T4EDOOuRnC0MeKJFEDf5mDRXg/iRSJOsYg2JC5WFi4Ht4imuxAogYzqrYoCeuy2Vcn0fkD2oOGac6zVlkX6Q4Z+lAE36jxU+mvcTULHK7tBbpBfebN7I8tSm4xMjcZUF+F3h2Gkb3rQwkgx7GDf0wuPmIJ8uW5dfuDVHsoOPl3VqEln/NZX2L/KhjzDhnl6vNHil1iqWZQLtu4nErNA9sUwU87nPsl+w==\",\n"
                + "        \"uuid\": \"be7e6b93-13b9-4b8f-89e3-0ad8258db94c\",\n"
                + "        \"data\": {\n"
                + "            \"orderid\": \"7520047953\",\n"
                + "            \"verified\": \"0\",\n"
                + "            \"accountid\": \"4052851907\",\n"
                + "            \"messageid\": \"100137003A703263176\",\n"
                + "            \"attributes\": {\n"
                + "            }\n"
                + "        }\n"
                + "    }\n"
                + "}";
            var expectedSerialization = "accountid4052851907attributesmessageid100137003A703263176notificationidorderid7520047953verified0";

            var rpcRequest = JsonConvert.DeserializeObject<JsonRpcRequest<AccountNotificationData>>(requestJson);

            var serializer = new Serializer();
            var serializedData = serializer.SerializeData(rpcRequest.Params.Data);

            Assert.AreEqual(expectedSerialization, serializedData);
        }

        [Test]
        public void TestExpectedNotificationSerializationWithNullAttributes()
        {
            var requestJson = ""
                + "{\n"
                + "    \"method\": \"account\",\n"
                + "    \"version\": \"1.1\",\n"
                + "    \"params\": {\n"
                //     Incorrect signature
                + "        \"signature\": \"qP1KOXErOUguNVPtGhmvv2SxQzRUOOdaM7oH6Oh+T6V4jKXyScyIQLb7FD4iSmIPXnWLv5X8TmeOBCnJtsdDmW7bcueK65VVCzIjs88HxJC37axgTOWBn8T4EDOOuRnC0MeKJFEDf5mDRXg/iRSJOsYg2JC5WFi4Ht4imuxAogYzqrYoCeuy2Vcn0fkD2oOGac6zVlkX6Q4Z+lAE36jxU+mvcTULHK7tBbpBfebN7I8tSm4xMjcZUF+F3h2Gkb3rQwkgx7GDf0wuPmIJ8uW5dfuDVHsoOPl3VqEln/NZX2L/KhjzDhnl6vNHil1iqWZQLtu4nErNA9sUwU87nPsl+w==\",\n"
                + "        \"uuid\": \"be7e6b93-13b9-4b8f-89e3-0ad8258db94c\",\n"
                + "        \"data\": {\n"
                + "            \"orderid\": \"7520047953\",\n"
                + "            \"verified\": \"0\",\n"
                + "            \"accountid\": \"4052851907\",\n"
                + "            \"messageid\": \"100137003A703263176\"\n"
                + "        }\n"
                + "    }\n"
                + "}";
            var expectedSerialization = "accountid4052851907messageid100137003A703263176notificationidorderid7520047953verified0";

            var rpcRequest = JsonConvert.DeserializeObject<JsonRpcRequest<AccountNotificationData>>(requestJson);

            var serializer = new Serializer();
            var serializedData = serializer.SerializeData(rpcRequest.Params.Data);

            Assert.AreEqual(expectedSerialization, serializedData);
        }

        private Mock<HttpRequest> CreateMockDebitNotificationRequest(string method = "POST", string rpcMethod = "debit")
        {
            var mockRequest = new Mock<HttpRequest>();
            mockRequest.SetupAllProperties();

            this.SetHttpRequestProperties(mockRequest.Object, method, rpcMethod);

            return mockRequest;
        }

        private void SetHttpRequestProperties(HttpRequest request, string method = "POST", string rpcMethod = "debit")
        {
            Stream stream;

            if (rpcMethod == "debit")
            {
                stream = this.CreateMockDebitNotificationRequestBody(rpcMethod);
            }
            else
            {
                // We will fallback on a mocked debit notification if the Rpc Method is not known.
                // That way we will simulate a request, but the method is incorrect/unknown on server.
                stream = this.CreateMockDebitNotificationRequestBody(rpcMethod);
            }

            stream.Flush();
            stream.Position = 0;

            request.Body = stream;
            request.Method = method;
            request.Path = "/trustly/notifications";
        }

        private Stream CreateMockDebitNotificationRequestBody(String rpcMethod)
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
            return new MemoryStream(byteArray);
        }

        private Mock<HttpRequest> CreateMockAccountNotificationRequest(string method = "POST")
        {
            Mock<HttpRequest> mockRequest = new Mock<HttpRequest>();

            mockRequest.SetupAllProperties();

            mockRequest.Setup(x => x.Body).Returns(() =>
            {
                var json = JsonConvert.SerializeObject(
                    client.CreateRequestPackage(
                        new AccountNotificationData
                        {
                            MessageID = Guid.NewGuid().ToString(),
                            OrderID = Guid.NewGuid().ToString(),
                            NotificationID = Guid.NewGuid().ToString(),
                            AccountID = "123",
                            Verified = "1",
                            Attributes = new AccountNotificationDataAttributes
                            {
                                Clearinghouse = "SWEDEN",
                                Bank = "The Bank",
                                Descriptor = "**** *084057",
                                Lastdigits = "084057",
                                PersonID = "SE198201019876",
                                Name = "John Doe",
                                Address = "Examplestreet 1",
                                Zipcode = "12345",
                                City = "Examplecity",
                                DirectDebitMandate = 0
                            }
                        },
                        "account"
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