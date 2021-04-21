using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;
using Trustly.Api.Client;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.LDAP;

namespace Client.Tests
{
    public class RequestTests
    {
        [Test]
        public void TestDepositRequest()
        {
            var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            var publicKeyPath = Path.Combine(homePath, "client_test_public.pem");
            var privateKeyPath = Path.Combine(homePath, "client_test_private.pem");
            var usernamePath = Path.Combine(homePath, "client_username.txt");
            var passwordPath = Path.Combine(homePath, "client_password.txt");

            if (new[] { publicKeyPath, privateKeyPath, usernamePath, passwordPath }.Any(path => File.Exists(path) == false))
            {
                Console.WriteLine("--- CANNOT RUN ANY TEST REQUESTS AGAINST THE TEST API, SINCE THERE ARE MISSING CREDENTIAL FILES IN YOUR USER'S HOME DIRECTORY");
                return;
            }

            KeyChain keyChain;
            using (var clientPublicStream = new FileStream(publicKeyPath, FileMode.Open, FileAccess.Read))
            {
                using (var cientPrivateStream = new FileStream(privateKeyPath, FileMode.Open, FileAccess.Read))
                {
                    using (var trustlyKeyStream = KeyChain.GetTrustlyTestKeyStream())
                    {
                        keyChain = new KeyChain(clientPublicStream, cientPrivateStream, trustlyKeyStream);
                    }
                }
            }

            var client = TrustlyApiClient.CreateDefaultTestClient(
                keyChain,
                File.ReadAllText(usernamePath).Trim(),
                File.ReadAllText(passwordPath).Trim()
            );

            // TODO: Hide "username" and "password" from the general request data. It should be done automatically by the client code, by extending somehow.
            // TODO: Check if possible to wrap inside an object and say "everything inside here should be exploded"

            // ERROR_MALFORMED_NOTIFICATIONURL

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
                    //ShopperStatement = "Trustly Test Deposit"
                }
            });

            Assert.NotNull(response);
            Assert.IsFalse(string.IsNullOrEmpty(response.URL));
        }
    }
}