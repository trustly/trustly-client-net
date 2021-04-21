using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using WireMock.Server;
using WireMock.Settings;

namespace WiremockRecorder
{
    class Program
    {
        /// <summary>
        /// Run this program to start a proxy to the Trustly Test API.
        ///
        /// You will then find the generated mappings inside the target directory.
        /// Usually this is: ./bin/Debug/net5.0/__admin/mappings
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var server = WireMockServer.Start(new WireMockServerSettings
            {
                Urls = new[] { "http://+:5001" },

                UseSSL = true,
                CertificateSettings = new WireMockCertificateSettings
                {

                },

                ProxyAndRecordSettings = new ProxyAndRecordSettings
                {
                    Url = "https://test.trustly.com/api/1",

                    AllowAutoRedirect = true,
                    SaveMapping = true,
                    SaveMappingToFile = true
                }
            });

            Console.WriteLine("Listening to http://localhost:5001, proxying to https://test.trustly.com/api/1");
            Console.ReadLine();

            server.Stop();

            Console.WriteLine("Stopped listening");

            var directory = new DirectoryInfo("__admin/mappings");

            if (directory.Exists)
            {
                foreach (var file in directory.GetFiles())
                {
                    var fileContent = File.ReadAllText(file.FullName);
                    var mapping = JObject.Parse(fileContent);

                    var getCount = mapping
                        .SelectTokens("$.Request.Methods[*]")
                        .Select(t => string.Equals(t.Value<string>(), "GET", StringComparison.InvariantCultureIgnoreCase))
                        .Count();

                    if (getCount > 0)
                    {
                        file.Delete();
                    }
                    else
                    {
                        // Let's figure out a better name for the mappings


                        var method = mapping.SelectTokens("$.Request.Methods[0]").Value<string>();
                        var rpcMethod = "";
                        var rpcBodyChecksum = "";
                        var httpResponseStatus = "";

                        // Rename, so it's something like POST_Deposit_AFF123_200, POST_Deposit_FEF21C_400
                    }
                }
            }

            server.Stop();
        }
    }
}
