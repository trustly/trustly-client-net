using System.IO;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;
using Trustly.Api.Domain.Requests.Data;
using Trustly.Client;
using WireMock.Server;
using WireMock.Settings;

namespace Client.Tests
{
    public class Tests
    {
        private WireMockServer _server;

        [SetUp]
        public void Setup()
        {
            this._server = WireMockServer.Start(new WireMockServerSettings
            {
                Urls = new[] { "http://+:5001" },
                StartAdminInterface = true,
                ReadStaticMappings = true,
                WatchStaticMappings = true,
                WatchStaticMappingsInSubdirectories = true,
                FileSystemHandler = new WireMock.Handlers.LocalFileSystemHandler(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Wiremock"))
            });
        }

        [Test]
        public async void Test1()
        {

            var client = new HttpClient();

            var depositData = new DepositRequestData
            {
                Username = "foo",
                Password = "bar",
                NotificationURL = "http://localhost:5000",
                Attributes = new DepositRequestDataAttributes
                {
                    Amount = "100.00"
                }
            };

            var depositDataString = JsonConvert.SerializeObject(depositData);
            var requestContent = new StringContent(depositDataString, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("http://localhost:5001", requestContent);

            var i = 0;


            //var client = new DefaultClient();

            //NUnit.Framework.Assert.AreEqual("A Name-post", client.GetResponse());

            // TODO: Check response
        }
    }
}