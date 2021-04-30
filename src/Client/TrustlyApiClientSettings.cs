using System;
using System.IO;
using System.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;

namespace Trustly.Api.Client
{
    public class TrustlyApiClientSettings
    {
        internal static readonly string URL_TEST = "https://test.trustly.com/api/1";
        internal static readonly string URL_PRODUCTION = "https://api.trustly.com/1";

        public string URL { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }

        public AsymmetricKeyParameter ClientPublicKey { get; internal set; }
        public AsymmetricKeyParameter ClientPrivateKey { get; internal set; }

        public AsymmetricKeyParameter TrustlyPublicKey { get; internal set; }

        private TrustlyApiClientSettings()
        {
        }

        public static TrustlyApiClientSettings ForDefaultProduction(
            string username = null,
            string password = null,
            string publicKeyPath = null,
            string privateKeyPath = null)
        {
            return ForDefaultCustom(URL_PRODUCTION, username, password, publicKeyPath, privateKeyPath);
        }

        public static TrustlyApiClientSettings ForDefaultTest(
            string username = null,
            string password = null,
            string publicKeyPath = null,
            string privateKeyPath = null)
        {
            return ForDefaultCustom(URL_TEST, username, password, publicKeyPath, privateKeyPath);
        }

        public static TrustlyApiClientSettings ForDefaultCustom(
            string url,
            string username = null,
            string password = null,
            string publicKeyPath = null,
            string privateKeyPath = null)
        {
            var settings = new WithEnvironment(new TrustlyApiClientSettings(), url);

            WithCredentials withCredentials;
            if (string.IsNullOrEmpty(username))
            {
                withCredentials = settings
                    .WithCredentialsFromUserHome();
            }
            else
            {
                withCredentials = settings
                    .WithCredentials(username, password);
            }

            WithClientCertificates withCertificates;
            if (string.IsNullOrEmpty(privateKeyPath))
            {
                withCertificates = withCredentials
                    .WithCertificatesFromUserHome();
            }
            else
            {
                withCertificates = withCredentials
                    .WithCertificatesFromFiles(publicKeyPath, privateKeyPath);
            }

            return withCertificates.AndTrustlyCertificate();
        }

        public static WithEnvironment ForProduction()
        {
            return new WithEnvironment(new TrustlyApiClientSettings(), URL_PRODUCTION);
        }

        public static WithEnvironment ForTest()
        {
            return new WithEnvironment(new TrustlyApiClientSettings(), URL_TEST);
        }

        public static WithEnvironment ForCustom(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("The URL must not be null nor empty");
            }

            return new WithEnvironment(new TrustlyApiClientSettings(), url);
        }



        /*

        public TrustlyApiClientSettings(bool test)
        {
            (test ? this.ForTest() : this.ForProduction())
                .WithSettingsFromDirectory(null);
        }

        public TrustlyApiClientSettings(bool test, string username, string password, Stream clientPublicKeyStream, Stream clientPrivateKeyStream)
        {
            (test ? this.ForTest() : this.ForProduction())
                .WithCredentials(username, password)
                .WithClientKeysFromStreams(clientPublicKeyStream, clientPrivateKeyStream);
        }

        public TrustlyApiClientSettings ForTest()
        {
            this.URL = URL_TEST;

            var clientAssembly = typeof(TrustlyApiClient).Assembly;
            return
        }

        public TrustlyApiClientSettings ForProduction()
        {
            this.URL = URL_PRODUCTION;

            var clientAssembly = typeof(TrustlyApiClient).Assembly;
            return
        }

        public TrustlyApiClientSettings WithUrl(string url)
        {
            this.URL = url;
            return this;
        }

        public TrustlyApiClientSettings WithCredentials(string username, string password)
        {
            this.Username = username;
            this.Password = password;
            return this;
        }

        public TrustlyApiClientSettings WithSettingsFromFiles(
                string clientPublicKeyPath,
                string clientPrivateKeyPath,
                string usernamePath,
                string passwordPath
            )
        {
            if (File.Exists(clientPublicKeyPath) == false)
                throw new ArgumentException($"Cannot create api settings since public key file {clientPublicKeyPath} is missing");
            if (File.Exists(clientPrivateKeyPath) == false)
                throw new ArgumentException($"Cannot create api settings since private key file {clientPrivateKeyPath} is missing");


            return
                this.WithClientKeysFromFiles(clientPublicKeyPath, clientPrivateKeyPath)
                .WithCredentials(
                    File.ReadAllText(usernamePath).Trim(),
                    File.ReadAllText(passwordPath).Trim()
                );
        }

        public TrustlyApiClientSettings WithSettingsFromDirectory(
                string directoryPath,
                string clientPublicKeyFileName = "trustly_client_public.pem",
                string clientPrivateKeyFileName = "trustly_client_private.pem",
                string clientUsernameFileName = "trustly_client_username.txt",
                string clientPsswordFileName = "trustly_client_password.txt"
            )
        {
            if (directoryPath == null)
            {
                directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }

            var publicKeyPath = Path.Combine(directoryPath, clientPublicKeyFileName);
            var privateKeyPath = Path.Combine(directoryPath, clientPrivateKeyFileName);
            var usernamePath = Path.Combine(directoryPath, clientUsernameFileName);
            var passwordPath = Path.Combine(directoryPath, clientPsswordFileName);

            return this.WithSettingsFromFiles(publicKeyPath, privateKeyPath, usernamePath, passwordPath);
        }

        public TrustlyApiClientSettings WithClientKeysFromFiles(string clientPublicKeyFilePath, string clientPrivateKeyFilePath)
        {
            using (var publicFileStream = new FileStream(clientPublicKeyFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var privateFileStream = new FileStream(clientPrivateKeyFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    return this.WithClientKeysFromStreams(publicFileStream, privateFileStream);
                }
            }
        }

        public TrustlyApiClientSettings WithClientKeysFromStreams(Stream clientPublicKeyStream, Stream clientPrivateKeyStream)
        {
            this.WithClientPublicKeyFromStream(clientPublicKeyStream);
            this.WithClientPrivateKeyFromStream(clientPrivateKeyStream);

            return this;
        }
        */
    }

    public class WithEnvironment
    {
        internal TrustlyApiClientSettings Settings { get; }

        public WithEnvironment(TrustlyApiClientSettings settings, string url)
        {
            this.Settings = settings;
            this.Settings.URL = url;
        }

        /// <summary>
        /// For internal use, do not use. You must supply credentials to be able to make requests.
        /// </summary>
        public WithCredentials WithoutCredentials()
        {
            return new WithCredentials(this.Settings, null, null);
        }

        public WithCredentials WithCredentials(string username, string password)
        {
            return new WithCredentials(this.Settings, username, password);
        }

        public WithCredentials WithCredentialsFromUserHome()
        {
            var directory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return this.WithCredentialsFromDirectory(directory);
        }

        public WithCredentials WithCredentialsFromDirectory(
            string directoryPath,
            string clientUsernameFileName = "trustly_client_username.txt",
            string clientPsswordFileName = "trustly_client_password.txt"
            )
        {
            var usernamePath = Path.Combine(directoryPath, clientUsernameFileName);
            var passwordPath = Path.Combine(directoryPath, clientPsswordFileName);

            return this.WithCredentialsFromFiles(usernamePath, passwordPath);
        }

        public WithCredentials WithCredentialsFromFiles(string usernamePath, string passwordPath)
        {
            if (File.Exists(usernamePath) == false)
                throw new ArgumentException($"Cannot create api settings since username key file {usernamePath} is missing");
            if (File.Exists(passwordPath) == false)
                throw new ArgumentException($"Cannot create api settings since password key file {passwordPath} is missing");

            return new WithCredentials(
                this.Settings,
                File.ReadAllText(usernamePath).Trim(),
                File.ReadAllText(passwordPath).Trim()
            );
        }
    }

    public class WithCredentials
    {
        public TrustlyApiClientSettings Settings { get; }

        public WithCredentials(TrustlyApiClientSettings settings, string username, string password)
        {
            this.Settings = settings;
            this.Settings.Username = username;
            this.Settings.Password = password;
        }

        public WithClientCertificates WithCertificatesFromUserHome(
            string clientPublicKeyFileName = "trustly_client_public.pem",
            string clientPrivateKeyFileName = "trustly_client_private.pem"
            )
        {
            var directory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return this.WithCertificatesFromDirectory(directory, clientPublicKeyFileName, clientPrivateKeyFileName);
        }

        public WithClientCertificates WithCertificatesFromDirectory(
            string directoryPath,
            string clientPublicKeyFileName = "trustly_client_public.pem",
            string clientPrivateKeyFileName = "trustly_client_private.pem"
            )
        {
            return this.WithCertificatesFromFiles(
                Path.Combine(directoryPath, clientPublicKeyFileName),
                Path.Combine(directoryPath, clientPrivateKeyFileName)
            );
        }

        public WithClientCertificates WithCertificatesFromFiles(string clientPublicKeyPath, string clientPrivateKeyPath)
        {
            using (var publicFileStream = new FileStream(clientPublicKeyPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var privateFileStream = new FileStream(clientPrivateKeyPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    return this.WithCertificatesFromStreams(publicFileStream, privateFileStream);
                }
            }
        }

        public WithClientCertificates WithCertificatesFromStreams(Stream publicFileStream, Stream privateFileStream)
        {
            using (var publicReader = new StreamReader(publicFileStream))
            {
                var publicPemReader = new PemReader(publicReader);
                var publicKeyPair = (AsymmetricKeyParameter)publicPemReader.ReadObject();

                using (var privateReader = new StreamReader(privateFileStream))
                {
                    var privatePemReader = new PemReader(privateReader);
                    var privateKeyPair = (AsymmetricCipherKeyPair)privatePemReader.ReadObject();

                    return new WithClientCertificates(this.Settings, publicKeyPair, privateKeyPair.Private);
                }
            }

            //return new WithTrustlyCertificates(this.Settings);
        }
    }

    public class WithClientCertificates
    {
        internal TrustlyApiClientSettings Settings { get; }

        public WithClientCertificates(TrustlyApiClientSettings settings, AsymmetricKeyParameter clientPublicKey, AsymmetricKeyParameter clientPrivateKey)
        {
            this.Settings = settings;

            this.Settings.ClientPrivateKey = clientPrivateKey;
            this.Settings.ClientPublicKey = clientPublicKey;
        }

        public TrustlyApiClientSettings AndTrustlyCertificate()
        {
            if (string.Equals(this.Settings.URL, TrustlyApiClientSettings.URL_PRODUCTION))
            {
                return this.AndTrustlyCertificateProduction();
            }
            else if (string.Equals(this.Settings.URL, TrustlyApiClientSettings.URL_TEST))
            {
                return this.AndTrustlyCertificateTest();
            }
            else
            {
                throw new ArgumentException("You can only automatically choose the Trustly certificate if you used the ForProduction() or ForTest() builder steps");
            }
        }

        public TrustlyApiClientSettings AndTrustlyCertificateProduction()
        {
            var clientAssembly = typeof(TrustlyApiClient).Assembly;
            return this.AndTrustlyCertificateFromStream(clientAssembly.GetManifestResourceStream("Trustly.Api.Client.Keys.trustly_live_key.cer"));
        }

        public TrustlyApiClientSettings AndTrustlyCertificateTest()
        {
            var clientAssembly = typeof(TrustlyApiClient).Assembly;
            return this.AndTrustlyCertificateFromStream(clientAssembly.GetManifestResourceStream("Trustly.Api.Client.Keys.trustly_test_key.cer"));
        }

        public TrustlyApiClientSettings AndTrustlyCertificateFromUserHome(string trustlyPublicKeyFileName = "trustly_public.pem")
        {
            var directory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return this.AndTrustlyCertificateFromDirectory(directory, trustlyPublicKeyFileName);
        }

        public TrustlyApiClientSettings AndTrustlyCertificateFromDirectory(string directoryPath, string trustlyPublicKeyFileName = "trustly_public.pem")
        {
            return this.AndTrustlyCertificateFromFile(
                Path.Combine(directoryPath, trustlyPublicKeyFileName)
            );
        }

        public TrustlyApiClientSettings AndTrustlyCertificateFromFile(string filePath)
        {
            using (var publicFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return this.AndTrustlyCertificateFromStream(publicFileStream);
            }
        }

        public TrustlyApiClientSettings AndTrustlyCertificateFromStream(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                var pemReader = new PemReader(reader);
                var keyPair = (AsymmetricKeyParameter)pemReader.ReadObject();

                this.Settings.TrustlyPublicKey = keyPair;
                if (this.Settings.TrustlyPublicKey == null)
                {
                    throw new ArgumentException($"Failed to load Trustly's public key from stream");
                }
            }

            return this.Settings;
        }
    }
}
