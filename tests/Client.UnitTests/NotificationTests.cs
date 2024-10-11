using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Trustly.Api.Domain;
using Trustly.Api.Domain.Exceptions;

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

        [Test]
        public async Task TestNotificationHandlerFromRequest()
        {
            var receivedDebitNotifications = 0;
            client.OnDebit += (sender, args) =>
            {
                receivedDebitNotifications++;
            };

            var mockRequest = this.CreateMockDebitNotificationRequest();
            await client.HandleNotificationFromRequestAsync(mockRequest.Object, str => Task.Delay(TimeSpan.MinValue));

            Assert.That(receivedDebitNotifications, Is.EqualTo(1));
        }

        [Test]
        public void TestUnionDeserialization()
        {
            var body = this.CreateMockDebitNotificationRequestBodyString("debit");
            var deserialized = JsonConvert.DeserializeObject<DebitNotification>(body);
            var serializer = JsonSerializer.CreateDefault();
            var data = deserialized.Params.Data.GetDebitDefaultNotificationData(serializer);

            Assert.That(data.EndUserID, Is.EqualTo("user@email.com"));
        }

        [Test]
        public async Task TestNotificationHandlerFromMiddlewareRequest()
        {
            var receivedDebitNotifications = 0;
            client.OnDebit += (sender, args) =>
            {
                receivedDebitNotifications++;
                args.Respond(new DebitNotificationResponseData
                {
                    Status = DebitNotificationResponseDataStatus.OK
                });
            };

            var mockRequest = this.CreateMockDebitNotificationRequest();
            var mockHttpContext = new Mock<HttpContext>();
            var mockResponse = new Mock<HttpResponse>();
            var responseStream = new MemoryStream();

            mockResponse.SetupAllProperties();
            mockResponse.Setup(_ => _.Headers).Returns(new HeaderDictionary());
            mockResponse.Setup(_ => _.Body).Returns(responseStream);

            mockHttpContext.Setup(_ => _.Request).Returns(mockRequest.Object);
            mockHttpContext.Setup(_ => _.Response).Returns(mockResponse.Object);

            await TrustlyApiClientExtensions.HandleNotificationRequest(mockHttpContext.Object, null, client);

            Assert.That(receivedDebitNotifications, Is.EqualTo(1));
            Assert.That(mockResponse.Object.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task TestNotificationHandlerFromMiddlewareRequestWithErrorResponse()
        {
            var receivedDebitNotifications = 0;
            client.OnDebit += (sender, args) =>
            {
                receivedDebitNotifications++;
                throw new InvalidOperationException("Things went badly");
            };

            var mockHttpContext = new DefaultHttpContext();
            this.SetHttpRequestProperties(mockHttpContext.Request);
            mockHttpContext.Response.Body = new MemoryStream();

            await TrustlyApiClientExtensions.HandleNotificationRequest(mockHttpContext, null, client);

            Assert.That(receivedDebitNotifications, Is.EqualTo(1));
            Assert.That(mockHttpContext.Response.StatusCode, Is.EqualTo(500));

            mockHttpContext.Response.Body.Position = 0;
            using (var sr = new StreamReader(mockHttpContext.Response.Body))
            {
                var bodyString = sr.ReadToEnd();
                Assert.That(bodyString, Is.EqualTo(TrustlyApiClientExtensions.GENERIC_ERROR_MESSAGE));
            }
        }

        [Test]
        public async Task TestNotificationHandlerFromMiddlewareRequestWithInternalErrorResponse()
        {
            var receivedDebitNotifications = 0;
            var previous = client.Settings.IncludeExceptionMessageInNotificationResponse;
            try
            {
                client.Settings.IncludeExceptionMessageInNotificationResponse = true;
                client.OnDebit += (sender, args) =>
                {
                    receivedDebitNotifications++;
                    throw new InvalidOperationException("Things went badly");
                };

                var mockHttpContext = new DefaultHttpContext();
                this.SetHttpRequestProperties(mockHttpContext.Request);
                mockHttpContext.Response.Body = new MemoryStream();

                await TrustlyApiClientExtensions.HandleNotificationRequest(mockHttpContext, null, client);

                Assert.That(receivedDebitNotifications, Is.EqualTo(1));
                Assert.That(500, Is.EqualTo(mockHttpContext.Response.StatusCode));

                mockHttpContext.Response.Body.Position = 0;
                using (var sr = new StreamReader(mockHttpContext.Response.Body))
                {
                    var bodyString = sr.ReadToEnd();
                    Assert.That(bodyString, Is.EqualTo("Things went badly"));
                }
            }
            finally
            {
                client.Settings.IncludeExceptionMessageInNotificationResponse = previous;
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
                await TrustlyApiClientExtensions.HandleNotificationRequest(mockHttpContext.Object, null, client);
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
                await client.HandleNotificationFromRequestAsync(mockRequest.Object, str => Task.Delay(TimeSpan.MinValue));
            });

            Assert.That(receivedDebitNotifications, Is.EqualTo(0));
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

                Assert.That(args.Data.AdditionalProperties.ContainsKey("Amount"), Is.False);
                Assert.That(args.Data.AdditionalProperties.ContainsKey("EnduserID"), Is.False);

                Assert.That(args.Data.AdditionalProperties["Amount"], Is.Null);
                Assert.That(args.Data.AdditionalProperties["EnduserID"], Is.Null);
              
                Assert.That(args.Data.AdditionalProperties.Value<String>("amount"), Is.EqualTo("100.00"));
                Assert.That(args.Data.AdditionalProperties.Value<String>("enduserid"), Is.EqualTo("user@email.com"));
            };

            var mockRequest = this.CreateMockDebitNotificationRequest(rpcMethod: "blaha");
            await client.HandleNotificationFromRequestAsync(mockRequest.Object, str => Task.Delay(TimeSpan.MinValue));

            Assert.That(receivedDebitNotifications, Is.EqualTo(0));
            Assert.That(receivedUnknownNotifications, Is.EqualTo(1));
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
            await client.HandleNotificationFromRequestAsync(mockRequest.Object, str => Task.Delay(TimeSpan.MinValue));

            Assert.That(receivedCount, Is.EqualTo(1));
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
                + "            \"notificationid\": \"123\",\n"
                + "            \"attributes\": {\n"
                + "              \"bank\": \"Commerzbank\",\n"
                + "              \"descriptor\": \"****************441300\",\n"
                + "              \"lastdigits\": \"441300\",\n"
                + "              \"clearinghouse\": \"GERMANY\"\n"
                + "            }\n"
                + "        }\n"
                + "    }\n"
                + "}";
            var expectedSerialization = "accountid4052851907attributesbankCommerzbankclearinghouseGERMANYdescriptor****************441300lastdigits441300messageid100137003A703263176notificationid123orderid7520047953verified0";

            var rpcRequest = JsonConvert.DeserializeObject<AccountDefaultNotification>(requestJson, TrustlyApiClient.DEFAULT_SERIALIZER_SETTINGS);

            var serializer = new Serializer();
            var serializedData = serializer.SerializeData(rpcRequest.Params.Data, true);

            Assert.That(serializedData, Is.EqualTo(expectedSerialization));
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
                + "            \"notificationid\": \"123\",\n"
                + "            \"attributes\": {\n"
                + "            }\n"
                + "        }\n"
                + "    }\n"
                + "}";
            var expectedSerialization = "accountid4052851907attributesmessageid100137003A703263176notificationid123orderid7520047953verified0";

            var rpcRequest = JsonConvert.DeserializeObject<AccountDefaultNotification>(requestJson, TrustlyApiClient.DEFAULT_SERIALIZER_SETTINGS);

            var serializer = new Serializer();
            var serializedData = serializer.SerializeData(rpcRequest.Params.Data);

            Assert.That(serializedData, Is.EqualTo(expectedSerialization));
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
                + "            \"messageid\": \"100137003A703263176\",\n"
                + "            \"notificationid\": \"123\"\n"
                + "        }\n"
                + "    }\n"
                + "}";
            var expectedSerialization = "accountid4052851907messageid100137003A703263176notificationid123orderid7520047953verified0";

            var rpcRequest = JsonConvert.DeserializeObject<AccountDefaultNotification>(requestJson, TrustlyApiClient.DEFAULT_SERIALIZER_SETTINGS);

            var serializer = new Serializer();
            var serializedData = serializer.SerializeData(rpcRequest.Params.Data);

            Assert.That(serializedData, Is.EqualTo(expectedSerialization));
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

        public class DebitIshNotificationRequest : JsonRpcNotification<DebitDefaultNotificationData, JsonRpcNotificationParams<DebitDefaultNotificationData>>
        {
            public DebitIshNotificationRequest(string rpcMethod) : base(rpcMethod) { }
        }

        private string CreateMockDebitNotificationRequestBodyString(string rpcMethod)
        {
            var debitParamsData = new DebitDefaultNotificationData
            {
                Amount = "100.00",
                Currency = "EUR",
                EndUserID = "user@email.com",
                MessageID = Guid.NewGuid().ToString(),
                OrderID = Guid.NewGuid().ToString(),
                NotificationID = Guid.NewGuid().ToString(),
                Timestamp = "2021-01-01 01:01:01"
            };

            var debitParams = new JsonRpcNotificationParams<DebitDefaultNotificationData>
            {
                UUID = Guid.NewGuid().ToString(),
                Data = debitParamsData,
            };

            var debitNotification = new DebitIshNotificationRequest(rpcMethod)
            {
                 Params = debitParams,
            };

            debitNotification.Params.Signature = this.client.Signer.CreateSignature(
                debitNotification.Method,
                debitNotification.Params.UUID,
                debitNotification.Params.Data
            );

            return JsonConvert.SerializeObject(debitNotification, TrustlyApiClient.DEFAULT_SERIALIZER_SETTINGS);
        }

        private Stream CreateMockDebitNotificationRequestBody(string rpcMethod)
        {
            var json = this.CreateMockDebitNotificationRequestBodyString(rpcMethod);
            var byteArray = Encoding.UTF8.GetBytes(json);
            return new MemoryStream(byteArray);
        }

        private Mock<HttpRequest> CreateMockAccountNotificationRequest(string method = "POST")
        {
            Mock<HttpRequest> mockRequest = new Mock<HttpRequest>();

            mockRequest.SetupAllProperties();

            mockRequest.Setup(x => x.Body).Returns(() =>
            {
                var mandateNotification = new AccountMandateNotification {
                     Params = new AccountMandateNotificationParams {
                         UUID = Guid.NewGuid().ToString(),
                         Data = new AccountMandateNotificationData {
                            MessageID = Guid.NewGuid().ToString(),
                            OrderID = Guid.NewGuid().ToString(),
                            NotificationID = Guid.NewGuid().ToString(),
                            AccountID = "123",
                            Verified = StringBoolean.TRUE,
                            Attributes = new AccountMandateNotificationDataAttributes
                            {
                                ClearingHouse = "SWEDEN",
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
                         }
                     }
                };

                mandateNotification.Params.Signature = this.client.Signer.CreateSignature(
                    mandateNotification.Method,
                    mandateNotification.Params.UUID,
                    mandateNotification.Params.Data
                );

                var json = JsonConvert.SerializeObject(mandateNotification, TrustlyApiClient.DEFAULT_SERIALIZER_SETTINGS);

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